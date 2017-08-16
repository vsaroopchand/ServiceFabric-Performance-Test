using Common;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System.Text;
using Newtonsoft.Json;
using System.Net.WebSockets;

namespace Service3
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class Service3 : StatefulService, IServiceThree
    {
        private const string Service4Uri = "fabric:/SfPerfTest/Service4";
        private const string WcfEndpoint = "WcfServiceEndpoint";
        private const string SocketEndpoint = "SocketEndpoint";
        private const string AppPrefix = "Service3";
        private readonly string Service4SocketUri = $"ws://localhost:{Constants.SVC4_WS_PORT}/Service4/";

        public Service3(StatefulServiceContext context)
            : base(context)
        { }

        public async Task VisitByRemotingAsync(ServiceMessage message)
        {
            message.StampThree.Visited = true;
            message.StampThree.TimeNow = DateTime.UtcNow;

            var storage = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, ServiceMessage>>("storage1");
            using (var tx = this.StateManager.CreateTransaction())
            {
                await storage.AddAsync(tx, message.MessageId, message);
                await tx.CommitAsync();
            }

            var service = ServiceProxy.Create<IServiceFour>(new Uri(Service4Uri), new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(1));
            await service.VisitByRemotingAsync(message);
            
        }

        public async Task VisitWcfAsync(ServiceMessage message)
        {
            var fabricClient = new FabricClient(FabricClientRole.Admin);
            var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri(Service4Uri));
            var bindings = WcfUtility.CreateTcpClientBinding();
            var partitionResolver = ServicePartitionResolver.GetDefault();

            message.StampThree.Visited = true;
            message.StampThree.TimeNow = DateTime.UtcNow;

            foreach (var partition in partitionList)
            {
                var partitionInfo = (Int64RangePartitionInformation)partition.PartitionInformation;
                var wcfClientProxy = new WcfCommunicationClientFactory<IServiceFour>(clientBinding: bindings, servicePartitionResolver: partitionResolver);
                var wcfClient = new SvcFourWcfCommunicationClient(wcfClientProxy, new Uri(Service4Uri), new ServicePartitionKey(partitionInfo.HighKey), listenerName: "WcfTcp");
                await wcfClient.InvokeWithRetryAsync(client => client.Channel.VisitWcfAsync(message));
            }
        }

        private async Task VisitSocketAsync(ServiceMessage message)
        {
            ClientWebSocket cws = new ClientWebSocket();
            byte[] receiveBuffer = new byte[102400];

            try
            {
                cws = new ClientWebSocket();
                await cws.ConnectAsync(new Uri(Service4SocketUri), CancellationToken.None);
                
                var messageJson = JsonConvert.SerializeObject(message);

                Task receiverTask = cws.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                Task sendTask = cws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(messageJson)), WebSocketMessageType.Binary, true, CancellationToken.None);

                await Task.WhenAll(receiverTask, sendTask);
            }
            finally
            {
                if(cws?.State == WebSocketState.Open || cws?.State == WebSocketState.Connecting)
                {
                    await cws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
            }
            
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] {
                new ServiceReplicaListener(this.CreateServiceRemotingListener, name: "Remoting"),
                new ServiceReplicaListener((ctx) =>
                {
                    return new WcfCommunicationListener<IServiceThree>(
                        wcfServiceObject: this,
                        serviceContext: ctx,
                        endpointResourceName: WcfEndpoint,
                        listenerBinding: WcfUtility.CreateTcpListenerBinding());
                }, name: "WcfTcp"),
                new ServiceReplicaListener((ctx) =>
                {
                    return new WsCommunicationListener(ctx, SocketEndpoint, AppPrefix, this.ProcessWsRequest);
                }, "WebSocket")
            };
        }
                
        //protected override async Task RunAsync(CancellationToken cancellationToken)
        //{
        //    // TODO: Replace the following sample code with your own logic 
        //    //       or remove this RunAsync override if it's not needed in your service.

        //    var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");
        //    var run = false;
        //    while (run = true)
        //    {
        //        cancellationToken.ThrowIfCancellationRequested();

        //        using (var tx = this.StateManager.CreateTransaction())
        //        {
        //            var result = await myDictionary.TryGetValueAsync(tx, "Counter");

        //            ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
        //                result.HasValue ? result.Value.ToString() : "Value does not exist.");

        //            await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

        //            // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
        //            // discarded, and nothing is saved to the secondary replicas.
        //            await tx.CommitAsync();
        //        }

        //        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        //    }
        //}

        void ProcessWsRequest(byte[] data, CancellationToken token, Action<byte[]> callback)
        {
            if (!token.IsCancellationRequested)
            {
                try
                {
                    var package = Encoding.UTF8.GetString(data);                    
                    var message = JsonConvert.DeserializeObject<ServiceMessage>(package);
                    message.StampThree.Visited = true;
                    message.StampThree.TimeNow = DateTime.UtcNow;

                    VisitSocketAsync(message)
                        .GetAwaiter()
                        .GetResult();

                    var responsePackage = JsonConvert.SerializeObject(message);                    
                    callback(Encoding.UTF8.GetBytes(responsePackage));
                }
                catch (Exception e)
                {
                    callback?.Invoke(Encoding.UTF8.GetBytes(e.Message));
                }
            }
        }
    }
}
