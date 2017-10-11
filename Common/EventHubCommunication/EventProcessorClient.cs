using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common.EventHubCommunication
{
    public class EventprocessorClientOptions
    {
        public string ConnString { get; set; }
        public string HubPath { get; set; }
        public string ConsumerGroup { get; set; } = EventHubConsumerGroup.DefaultGroupName;
        public string Host { get; set; } = Environment.MachineName;
        public string StorageConnString { get; set; }

        public Action<string> OnMessageAction;

        public Action<Exception> OnError;
    }

    public class EventProcessorClient : IDisposable
    {
        Task backgroundTask;
        EventprocessorClientOptions _options;
        EventProcessorHost _host;

        public EventProcessorClient(EventprocessorClientOptions options)
        {
            _options = options;
        }

        public void Start()
        {
            this.backgroundTask = this.Listen();
        }

        private async Task Listen()
        {
            _host = new EventProcessorHost(_options.Host, _options.HubPath, _options.ConsumerGroup, _options.ConnString, _options.StorageConnString);      
            var factory = new EventProcessorFactory();
            factory.OnMessageAction += _options.OnMessageAction;
            factory.OnError += _options.OnError;

            /*
            Use this if you want to read from beginning of your EventHub stream
            await _host.RegisterEventProcessorFactoryAsync(factory);
            */

            
            await _host.RegisterEventProcessorFactoryAsync(factory, new EventProcessorOptions
            {
                InitialOffsetProvider = (partitionId) => DateTime.UtcNow,
            });            
        }

        public async Task Stop()
        {
            await _host.UnregisterEventProcessorAsync();

            if (backgroundTask != null)
            {
                backgroundTask.Dispose();
                backgroundTask = null;
            }
        }
        public void Dispose()
        {
            this.Stop()
                .GetAwaiter()
                .GetResult();
        }
    }
}
