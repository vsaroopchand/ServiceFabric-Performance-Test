using Common;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Net.Http;
using System.Net.Http.Headers;
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
        private const string AppPrefix = "Service4";
        public Service4(StatefulServiceContext context)
            : base(context)
        { }

        public async Task VisitByRemotingAsync(ServiceMessage message)
        {
            message.StampFour.Visited = true;
            message.StampFour.TimeNow = DateTime.UtcNow;

            var storage = await StateManager.GetOrAddAsync<IReliableDictionary<string, ServiceMessage>>("storage");
            using (var tx = StateManager.CreateTransaction())
            {
                await storage.AddAsync(tx, message.MessageId, message);
                await tx.CommitAsync();
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                     new MediaTypeWithQualityHeaderValue("application/json"));

                var package = JsonConvert.SerializeObject(message, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                using (var response = await client.PostAsync(await GetWebApiAddress(), new StringContent(package, System.Text.Encoding.UTF8, "application/json")))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        public async Task VisitWcfAsync(ServiceMessage message)
        {
            await VisitByRemotingAsync(message);
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
            return new[] {
                new ServiceReplicaListener(this.CreateServiceRemotingListener, name: "Remoting"),
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
                new ServiceReplicaListener((ctx) => { return new ServiceBusTopicListener(ctx, ProcessTopicMessage, LogError); }, "PubSub")
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

                    VisitByRemotingAsync(message)
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
            VisitByRemotingAsync(message).GetAwaiter().GetResult();
            ServiceBusSenderClient.Send("end", message, LogError);
        }

        void LogError(Exception e)
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, e.Message);
        }
    }
}
