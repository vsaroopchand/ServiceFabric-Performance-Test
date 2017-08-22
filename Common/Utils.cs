using Microsoft.ServiceFabric.Services.Client;
using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public static class Utils
    {
        public static async Task<string> GetSocketEndpoint(string serviceName, StatefulServiceContext context)
        {
            var client = new FabricClient(FabricClientRole.Admin);
            var servicePartitionResolver = ServicePartitionResolver.GetDefault();
            var serviceUri = context.CodePackageActivationContext.ApplicationName + "/" + serviceName;
            var partitionList = await client.QueryManager.GetPartitionListAsync(new Uri(serviceUri));
            var socketAddress = string.Empty;

            foreach (var partition in partitionList)
            {
                long partitionKey = ((Int64RangePartitionInformation)partition.PartitionInformation).HighKey;
                var resolvedPartition = await servicePartitionResolver.ResolveAsync(new Uri(serviceUri), new ServicePartitionKey(partitionKey), CancellationToken.None);
                var endpoint = resolvedPartition.GetEndpoint();
                var endpointAddresses = endpoint.Address.Split(',');
                foreach (var address in endpointAddresses)
                {
                    if (address.Contains("WebSocket"))
                    {
                        var addressParts = address.Replace("{", "").Replace("}", "").Replace("\\", "").Split(':');
                        socketAddress = $"ws:{addressParts[2]}:{addressParts[3]}";
                    }
                }
            }

            return socketAddress;
        }
    }
}
