using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class EventHubSender
    {

        private readonly string _path;
        private readonly string _connString;

        public EventHubSender(string connString, string path)
        {
            _connString = connString;
            _path = path;
        }

        public async Task Send(ServiceMessage message)
        {
            var nsm = NamespaceManager.CreateFromConnectionString(_connString);

            EventHubDescription desc = new EventHubDescription(_path);
            await nsm.CreateEventHubIfNotExistsAsync(desc);

            var client = EventHubClient.CreateFromConnectionString(_connString, desc.Path);
            client.RetryPolicy = new RetryExponential(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), 5);

            var json = JsonConvert.SerializeObject(message);
            var bytes = Encoding.UTF8.GetBytes(json);

            await client.SendAsync(new EventData(bytes));
        }

    }
}
