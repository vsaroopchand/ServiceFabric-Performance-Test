using Common;
using Common.DotNettyCommunication;
using Common.Grpc;
using Common.RemotingV2.CustomSeriaizer;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service2
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class Service2 : StatefulService, IServiceTwo
    {
        private const string Service3Uri = "fabric:/SfPerfTest/Service3";
        private const string WcfEndpoint = "WcfServiceEndpoint";
        private const string SocketEndpoint = "SocketEndpoint";
        private const string DotNettySimpleTcpEndpoint = "dotnetty-simple-tcp";
        private const string GrpcEndpoint = "GrpcServiceEndpoint";
        private const string AppPrefix = "Service2";
        private readonly string Service3SocketUri = $"ws://localhost:{Constants.SVC3_WS_PORT}/Service3/";
        private ClientWebSocket cws;

        public Service2(StatefulServiceContext context)
            : base(context)
        {

#pragma warning disable CS0618 // Type or member is obsolete
            if (!this.StateManager.TryAddStateSerializer(new JsonNetServiceMessageSerializer()))
#pragma warning restore CS0618 // Type or member is obsolete
            {

            }
        }

        public async Task VisitByRemotingAsync(ServiceMessage message)
        {
            message.StampTwo.Visited = true;
            message.StampTwo.TimeNow = DateTime.UtcNow;

            var storage = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, ServiceMessage>>("storage1");
            using (var tx = this.StateManager.CreateTransaction())
            {
                await storage.AddAsync(tx, message.MessageId, message);
                await tx.CommitAsync();
            }

            //var service = ServiceProxy.Create<IServiceThree>(new Uri(Service3Uri), new ServicePartitionKey(1));

            var proxyFactory = new ServiceProxyFactory((c) =>
            {
                return new FabricTransportServiceRemotingClientFactory(serializationProvider: new ServiceRemotingJsonSerializationProvider());
            });
            var service = proxyFactory.CreateServiceProxy<IServiceThree>(new Uri(Service3Uri), new ServicePartitionKey(1), listenerName: "RemotingV2");

            await service.VisitByRemotingAsync(message);

        }

        public async Task VisitWcfAsync(ServiceMessage message)
        {
            var fabricClient = new FabricClient(FabricClientRole.Admin);
            var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri(Service3Uri));
            var bindings = WcfUtility.CreateTcpClientBinding();
            var partitionResolver = ServicePartitionResolver.GetDefault();

            message.StampTwo.Visited = true;
            message.StampTwo.TimeNow = DateTime.UtcNow;

            foreach (var partition in partitionList)
            {
                var partitionInfo = (Int64RangePartitionInformation)partition.PartitionInformation;
                var wcfClientProxy = new WcfCommunicationClientFactory<IServiceThree>(clientBinding: bindings, servicePartitionResolver: partitionResolver);
                var wcfClient = new SvcThreeWcfCommunicationClient(wcfClientProxy, new Uri(Service3Uri), new ServicePartitionKey(partitionInfo.HighKey), listenerName: "WcfTcp");
                await wcfClient.InvokeWithRetryAsync(client => client.Channel.VisitWcfAsync(message));
            }
        }

        private async Task VisitSocketAsync(ServiceMessage message)
        {
            
            byte[] receiveBuffer = new byte[102400];

            try
            {
                if(cws == null)
                {
                    cws = new ClientWebSocket();
                    var endpoint = await Utils.GetSocketEndpoint("Service3", this.Context);
                    await cws.ConnectAsync(new Uri(endpoint), CancellationToken.None);

                }
                //cws = new ClientWebSocket();

                //var endpoint = await Utils.GetSocketEndpoint("Service3", this.Context);

                //await cws.ConnectAsync(new Uri(endpoint), CancellationToken.None);

                var messageJson = JsonConvert.SerializeObject(message);

                Task receiverTask = cws.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                Task sendTask = cws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(messageJson)), WebSocketMessageType.Binary, true, CancellationToken.None);

                await Task.WhenAll(receiverTask, sendTask);
            }
            catch(Exception e)
            {
                LogError(e);
            }

            //finally
            //{
            //    if (cws?.State == WebSocketState.Open || cws?.State == WebSocketState.Connecting)
            //    {
            //        await cws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            //    }
            //}

        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            var grpcServices = new[] { GrpcMessageService.BindService(new GrpcMessageServiceImpl(this.Context, ProcessGrpcMessage)) };

            return new[] {
                //new ServiceReplicaListener(this.CreateServiceRemotingListener, name: "Remoting"),
                new ServiceReplicaListener((ctx) =>
                 {
                     return new FabricTransportServiceRemotingListener(ctx, this, serializationProvider: new ServiceRemotingJsonSerializationProvider());

                 }, name: "RemotingV2"),
                new ServiceReplicaListener((ctx) =>
                {
                    return new WcfCommunicationListener<IServiceTwo>(
                        wcfServiceObject: this,
                        serviceContext: ctx,
                        endpointResourceName: WcfEndpoint,
                        listenerBinding: WcfUtility.CreateTcpListenerBinding());
                }, name: "WcfTcp"),
                new ServiceReplicaListener((ctx) =>
                {
                    return new WsCommunicationListener(ctx, SocketEndpoint, AppPrefix, this.ProcessWsRequest);
                }, "WebSocket"),
                new ServiceReplicaListener((ctx) => { return new ServiceBusTopicListener(ctx, ProcessTopicMessage, LogError, ServiceBusTopicReceiverType.Performance); }, "PubSub"),
                new ServiceReplicaListener((ctx) => { return new EventHubCommunicationListener(ctx, ProcessEventHubMessage, LogError); }, "EventHub"),
                new ServiceReplicaListener((ctx) => { return new SimpleCommunicationListener(ctx, DotNettySimpleTcpEndpoint, ProcessDotNettyMessage, LogError); }, "DotNettySimpleTcp"),
                new ServiceReplicaListener((ctx) => { return new GrpcCommunicationListener(ctx, grpcServices , LogMessage, GrpcEndpoint); }, "grpc")
            };
        }

        //protected override async Task OnOpenAsync(ReplicaOpenMode openMode, CancellationToken cancellationToken)
        //{                     
        //    await base.OnOpenAsync(openMode, cancellationToken);
        //}

        protected override async Task OnCloseAsync(CancellationToken cancellationToken)
        {
            if (cws?.State == WebSocketState.Open || cws?.State == WebSocketState.Connecting)
            {
                await cws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }

            await base.OnCloseAsync(cancellationToken);
        }

        protected override async Task OnChangeRoleAsync(ReplicaRole newRole, CancellationToken cancellationToken)
        {
            if(newRole != ReplicaRole.Primary)
            {
                if (cws?.State == WebSocketState.Open || cws?.State == WebSocketState.Connecting)
                {
                    await cws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
            }

            await base.OnChangeRoleAsync(newRole, cancellationToken);
        }

        void ProcessWsRequest(byte[] data, CancellationToken token, Action<byte[]> callback)
        {
            if (!token.IsCancellationRequested)
            {
                try
                {
                    var package = Encoding.UTF8.GetString(data);
                    var message = JsonConvert.DeserializeObject<ServiceMessage>(package);
                    message.StampTwo.Visited = true;
                    message.StampTwo.TimeNow = DateTime.UtcNow;

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
        
        void ProcessTopicMessage(ServiceMessage message)
        {
            message.StampTwo.Visited = true;
            message.StampTwo.TimeNow = DateTime.UtcNow;

            ConfigurationPackage configPackage = this.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            ConfigurationSection configSection = configPackage.Settings.Sections[Constants.SB_CONFIG_SECTION];
            var connString = (configSection.Parameters[Constants.SB_CONN_STRING]).Value;

            ServiceBusSenderClient2.Send(connString, "svc3", message, LogError)
                .GetAwaiter()
                .GetResult();
        }

        void ProcessEventHubMessage(string package)
        {
            var message = JsonConvert.DeserializeObject<ServiceMessage>(package);
            message.StampTwo.Visited = true;
            message.StampTwo.TimeNow = DateTime.UtcNow;

            ConfigurationPackage configPackage = this.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            ConfigurationSection configSection = configPackage.Settings.Sections[Constants.EH_CONFIG_SECTION];
            var connString = (configSection.Parameters[Constants.EH_CONN_STRING]).Value;
            var path = (configSection.Parameters[Constants.EH_SENDTO_HUB_PATH]).Value;

            try
            {
                new EventHubSender(connString, path).Send(message).GetAwaiter().GetResult();                
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        async Task ProcessDotNettyMessage(object package)
        {
            var message = JsonConvert.DeserializeObject<ServiceMessage>(package as string);
            message.StampTwo.Visited = true;
            message.StampTwo.TimeNow = DateTime.UtcNow;

            await SimpleClient.SendAsync(message, this.Context, "Service3", LogError);


            // need to do this here in order to avoid a COM error in runtime
            //var client = new FabricClient(FabricClientRole.User);
            //var servicePartitionResolver = ServicePartitionResolver.GetDefault();
            //var serviceUri = this.Context.CodePackageActivationContext.ApplicationName + "/" + "Service3";
            //var partitionList = client.QueryManager.GetPartitionListAsync(new Uri(serviceUri)).GetAwaiter().GetResult();

            //var destinationEndpoint = await SimpleClient.GetSocketEndpointAsync(new Uri(serviceUri), this.Context, partitionList);
            //await SimpleClient.SendAsync(message, destinationEndpoint.Item1, destinationEndpoint.Item2, LogError);                        
        }

        public void ProcessGrpcMessage(ServiceMessage2 message)
        {                
            message.StampTwo.Visited = true;
            message.StampTwo.TimeNow = DateTime.UtcNow.Ticks;
            GrpcClienHelper.SendMessage("Service3", message).GetAwaiter().GetResult(); ;
        }

        void LogError(Exception e)
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, e.Message);
        }
        void LogMessage(string message)
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, message);
        }
    }
}
