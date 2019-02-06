using Common;
using Common.DotNettyCommunication;
using Common.Grpc;
using Common.RemotingV2.CustomSeriaizer;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service4
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class Service4 : StatefulService, IServiceFour
    {
        private const string WcfEndpoint = "WcfServiceEndpoint";
        private const string SocketEndpoint = "SocketEndpoint";
        private const string DotNettySimpleTcpEndpoint = "dotnetty-simple-tcp";
        private const string GrpcEndpoint = "GrpcServiceEndpoint";
        private const string AppPrefix = "Service4";
        private const string webProxyUri = "fabric:/SfPerfTest/ProxyService";

        public Service4(StatefulServiceContext context)
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
            message.StampFour.Visited = true;
            message.StampFour.TimeNow = DateTime.UtcNow;

            var storage = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, ServiceMessage>>("storage");
            using (var tx = this.StateManager.CreateTransaction())
            {
                await storage.AddAsync(tx, message.MessageId, message);
                await tx.CommitAsync();
            }
            
            var proxyFactory = new ServiceProxyFactory((c) =>
            {
                //return new FabricTransportServiceRemotingClientFactory(serializationProvider: new ServiceRemotingJsonSerializationProvider());
                return new FabricTransportServiceRemotingClientFactory();
            });
            var service = proxyFactory.CreateServiceProxy<IWebProxyService>(new Uri(webProxyUri), new ServicePartitionKey(1), listenerName: "RemotingV2");

            await service.VisitByRemotingAsync(message);

        }


        public async Task EndWithHttpPost(ServiceMessage message)
        {
            message.StampFour.Visited = true;
            message.StampFour.TimeNow = DateTime.UtcNow;

            var storage = await StateManager.GetOrAddAsync<IReliableDictionary<string, ServiceMessage>>("storage");
            using (var tx = StateManager.CreateTransaction())
            {
                await storage.AddAsync(tx, message.MessageId, message);
                await tx.CommitAsync();
            }

            // .net 4.61 error https://github.com/dotnet/corefx/issues/22781
            //using (var client = new HttpClient())
            //{
            //    client.DefaultRequestHeaders.Accept.Add(
            //         new MediaTypeWithQualityHeaderValue("application/json"));

            //    var package = JsonConvert.SerializeObject(message, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            //    using (var response = await client.PostAsync(await GetWebApiAddress(), new StringContent(package, System.Text.Encoding.UTF8, "application/json")))
            //    {
            //        response.EnsureSuccessStatusCode();
            //    }
            //}
            var endpoint = await GetWebApiAddress();
            WebRequest wr = WebRequest.Create(endpoint);
            wr.Method = "POST";
            wr.ContentType = "application/json";
            var bodyJson = JsonConvert.SerializeObject(message, Formatting.Indented, settings: new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            var body = Encoding.UTF8.GetBytes(bodyJson);
            wr.ContentLength = body.Length;


            using(var stream = wr.GetRequestStream())
            {
                stream.Write(body, 0, body.Length);
            }
            
        }

        public async Task VisitWcfAsync(ServiceMessage message)
        {
            await EndWithHttpPost(message);
        }

        public async Task<string> GetWebApiAddress()
        {
            var serviceUri = this.Context.CodePackageActivationContext.ApplicationName + "/" + "ProxyService";
            var fabClient = new FabricClient(FabricClientRole.Admin);
            var partitionList = await fabClient.QueryManager.GetPartitionListAsync(new Uri(serviceUri));
            var reverseProxyPort = 19081;
            var proxyUrl = string.Empty;
            foreach(var partition in partitionList)
            {
                long partitionKey = ((Int64RangePartitionInformation)partition.PartitionInformation).LowKey;
                proxyUrl =
                    $"http://localhost:{reverseProxyPort}/{serviceUri.Replace("fabric:/", "")}/api/remoting/end?PartitionKind={partition.PartitionInformation.Kind}&PartitionKey={partitionKey}";
               
            }

            return proxyUrl;
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            var grpcServices = new[] { GrpcMessageService.BindService(new GrpcMessageServiceImpl(this.Context, ProcessGrpcMessage)) };


            return new[] {
                //new ServiceReplicaListener(this.CreateServiceRemotingListener, name: "Remoting"),
                new ServiceReplicaListener((ctx) =>
                 {
                     //return new FabricTransportServiceRemotingListener(ctx, this, serializationProvider: new ServiceRemotingJsonSerializationProvider());
                     return new FabricTransportServiceRemotingListener(ctx, this);

                 }, name: "RemotingV2"),
                new ServiceReplicaListener((ctx) =>
                {
                    return new WcfCommunicationListener<IServiceFour>(
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

        void ProcessWsRequest(byte[] data, CancellationToken token, Action<byte[]> callback)
        {
            if (!token.IsCancellationRequested)
            {
                try
                {
                    var package = Encoding.UTF8.GetString(data);
                    var message = JsonConvert.DeserializeObject<ServiceMessage>(package);

                    EndWithHttpPost(message)
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
            EndWithHttpPost(message).GetAwaiter().GetResult();

            ConfigurationPackage configPackage = this.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            ConfigurationSection configSection = configPackage.Settings.Sections[Constants.SB_CONFIG_SECTION];
            var connString = (configSection.Parameters[Constants.SB_CONN_STRING]).Value;

            ServiceBusSenderClient2.Send(connString, "end", message, LogError)
                .GetAwaiter()
                .GetResult();
        }

        void ProcessEventHubMessage(string package)
        {
            var message = JsonConvert.DeserializeObject<ServiceMessage>(package);

            EndWithHttpPost(message).GetAwaiter().GetResult();

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
            message.StampFour.Visited = true;
            message.StampFour.TimeNow = DateTime.UtcNow;
            
            await EndWithHttpPost(message);
        }

        public void ProcessGrpcMessage(ServiceMessage2 message)
        {
            message.StampFour.Visited = true;
            message.StampFour.TimeNow = DateTime.UtcNow.Ticks;

            // convert protobuffer message to Datacontract and send it back to web proxy using http
            var m2 = new ServiceMessage
            {
                MessageId = message.MessageId,
                MessageJson = message.MessageJson,
                SessionId = message.SessionId,
                CommChannel = message.CommChannel,
                StampOne = new Common.VisitStamp
                {
                    Visited = message.StampOne.Visited,
                    TimeNow = new DateTime(message.StampOne.TimeNow)
                },
                StampTwo = new Common.VisitStamp
                {
                    Visited = message.StampTwo.Visited,
                    TimeNow = new DateTime(message.StampTwo.TimeNow)
                },
                StampThree = new Common.VisitStamp
                {
                    Visited = message.StampThree.Visited,
                    TimeNow = new DateTime(message.StampThree.TimeNow)
                },
                StampFour = new Common.VisitStamp
                {
                    Visited = true,
                    TimeNow = DateTime.UtcNow,
                },
            };

            EndWithHttpPost(m2).GetAwaiter().GetResult();
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
