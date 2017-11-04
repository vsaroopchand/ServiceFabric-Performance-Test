using Common.Grpc.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using System;
using System.Fabric;
using System.Threading.Tasks;

namespace Common.Grpc
{
    public static class GrpcClienHelper
    {
        public static async Task<Noop> SendMessage(string destinationService, ServiceMessage2 message)
        {
            var client = new FabricClient(FabricClientRole.Admin);
            var resolver = ServicePartitionResolver.GetDefault();
            var serviceUri = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/" + destinationService);
            var communicationFactory = new GrpcCommunicationClientFactory<Common.Grpc.GrpcMessageService.GrpcMessageServiceClient>(null, resolver);
            var partitionList = await client.QueryManager.GetPartitionListAsync(serviceUri);

            foreach (var partition in partitionList)
            {
                long partitionKey = ((Int64RangePartitionInformation)partition.PartitionInformation).HighKey;
                var partitionClient = new ServicePartitionClient<GrpcCommunicationClient<Common.Grpc.GrpcMessageService.GrpcMessageServiceClient>>(communicationFactory, serviceUri, new ServicePartitionKey(partitionKey), listenerName: "grpc");
                var reply = partitionClient.InvokeWithRetry((communicationClient) => communicationClient.Client.Send(message));
            }

            return new Noop();
        }
    }
}
