using DotNetty.Codecs;
using DotNetty.Codecs.Base64;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json;
using System;
using System.Fabric;
using System.Fabric.Query;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Common.DotNettyCommunication
{
    public static class SimpleClient
    {
        public static async Task SendAsync(ServiceMessage message, string host, int port, Action<Exception> exceptionHandler)
        {
            try
            {
                await SendAsync(message, host, port);
            }
            catch (Exception e)
            {
                exceptionHandler?.Invoke(e);
            }
        }
        public static async Task SendAsync(ServiceMessage message, StatefulServiceContext ctx, string destinationService, Action<Exception> exceptionHandler)
        {
            try
            {                
                Tuple<string, int> address = await GetSocketEndpointAsync(destinationService, ctx);
                await SendAsync(message, address.Item1, address.Item2);
            }
            catch(Exception e)
            {
                exceptionHandler?.Invoke(e);
            }
        }

        private static async Task SendAsync(ServiceMessage message, string host, int port)
        {
            var group = new MultithreadEventLoopGroup();
            var bootstrap = new Bootstrap();

            bootstrap
                .Group(group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast(new LoggingHandler());
                    pipeline.AddLast(new StringEncoder(Encoding.UTF8), new StringDecoder(Encoding.UTF8));

                }));

            IChannel clientChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(host), port));

            var payload = JsonConvert.SerializeObject(message);
            await clientChannel.WriteAndFlushAsync(payload);
        }

        public static async Task<Tuple<string, int>> GetSocketEndpointAsync(Uri serviceUri, StatefulServiceContext context, ServicePartitionList partitionList)
        {
            var client = new FabricClient(FabricClientRole.User);
            var servicePartitionResolver = ServicePartitionResolver.GetDefault();
            var address = new Tuple<string, int>("", 0);

            foreach (var partition in partitionList)
            {
                long partitionKey = ((Int64RangePartitionInformation)partition.PartitionInformation).HighKey;
                var resolvedPartition = await servicePartitionResolver.ResolveAsync(serviceUri, new ServicePartitionKey(partitionKey), CancellationToken.None);
                var endpoint = resolvedPartition.GetEndpoint();
                address = FindAddress(endpoint, Constants.DOTNETTY_SIMPLE_ENDPOINT);
            }

            return address;

        }
        public static async Task<Tuple<string, int>> GetSocketEndpointAsync(string serviceName, StatefulServiceContext context)
        {
            var client = new FabricClient(FabricClientRole.User);
            var servicePartitionResolver = ServicePartitionResolver.GetDefault();
            var serviceUri = context.CodePackageActivationContext.ApplicationName + "/" + serviceName;
            var partitionList = await client.QueryManager.GetPartitionListAsync(new Uri(serviceUri));
            var address = new Tuple<string, int>("", 0);

            foreach (var partition in partitionList)
            {
                long partitionKey = ((Int64RangePartitionInformation)partition.PartitionInformation).HighKey;
                var resolvedPartition = await servicePartitionResolver.ResolveAsync(new Uri(serviceUri), new ServicePartitionKey(partitionKey), CancellationToken.None);
                var endpoint = resolvedPartition.GetEndpoint();
                address = FindAddress(endpoint, Constants.DOTNETTY_SIMPLE_ENDPOINT);
            }

            return address;
        }

        private static Tuple<string, int> FindAddress(ResolvedServiceEndpoint endpoint, string endpointName)
        {
            var endpointAddresses = endpoint.Address.Split(',');
            var result = new Tuple<string, int>("", 0);

            foreach (var address in endpointAddresses)
            {
                if (address.Contains(endpointName))
                {
                    var addressParts = address.Replace("{", "").Replace("}", "").Replace("\\", "").Split(':');                    
                    var host = addressParts[2].Replace("//", "");
                    var portStr = addressParts[3].Replace("/", "");

                    int port;
                    if (!int.TryParse(portStr, out port))
                    {
                        var sb = new StringBuilder();
                        foreach (char c in portStr)
                        {
                            if (Char.IsNumber(c))
                            {
                                sb.Append(c);
                            }
                        }
                        port = int.Parse(sb.ToString());
                    }

                    result = new Tuple<string, int>(host, port);
                }               
            }

            return result;
        }
    }
}
