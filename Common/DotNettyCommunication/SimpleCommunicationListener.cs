using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Common.DotNettyCommunication
{
    public class SimpleCommunicationListener : ICommunicationListener
    {
        private readonly Func<object, Task> messageHandler;
        private readonly Action<Exception> exceptionHandler;
        private readonly string listeningAddress, publicAddress, host;
        private readonly int port;

        CancellationTokenSource serverControl;        
        Bootstrapper bootStrapper;        
        Task runTask = Task.FromResult<object>(null);

        public SimpleCommunicationListener(StatefulServiceContext context, string endpointName, Func<object, Task> messageHandler, Action<Exception> exceptionHandler)
        {
            this.messageHandler = messageHandler;
            this.exceptionHandler = exceptionHandler;

            // need to extract host and port from context and environment
            //int minWorkerThreads;
            //int minCompletionPortThreads;

            //ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThreads);
            //ThreadPool.SetMinThreads(minWorkerThreads, Math.Max(5, minCompletionPortThreads));

            EndpointResourceDescription endpointDesc = context.CodePackageActivationContext.GetEndpoint(endpointName);
     
            port = endpointDesc.Port;
            var node = FabricRuntime.GetNodeContext();
            host = FabricRuntime.GetNodeContext().IPAddressOrFQDN;
            if(host == "localhost")
            {
                var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                var internalIP = hostEntry.AddressList.ToList().FirstOrDefault(t => t.AddressFamily == AddressFamily.InterNetwork);
                if(internalIP != null)
                {                    
                   host = internalIP.ToString();                    
                }
            }

            listeningAddress = $"tcp://+:{endpointDesc.Port}";
            publicAddress = listeningAddress.Replace("+", host);

            this.serverControl = new CancellationTokenSource();
        }

        public void Abort()
        {
            try
            {
                this.serverControl.Cancel(false);
                this.bootStrapper.ClosedCompletion.Wait(TimeSpan.FromSeconds(10));
                this.runTask.Wait(300, CancellationToken.None);
            }
            catch(Exception ex)
            {
                this.exceptionHandler?.Invoke(ex);
            }
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            try
            {
                this.serverControl.Cancel(false);
                await this.bootStrapper.ClosedCompletion.ConfigureAwait(false);
                await this.runTask.ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                this.exceptionHandler?.Invoke(ex);
            }
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            
            this.bootStrapper = new Bootstrapper(this.host, this.port, 
                async (message) => {                 
                    await messageHandler(message);                    
                }, 
                async (error) => {
                    this.exceptionHandler?.Invoke(error);
                }) ;

            this.runTask = this.bootStrapper.RunAsync(1, this.serverControl.Token);

            return Task.FromResult(publicAddress);
        }
    }
}
