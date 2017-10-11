using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Concurrent;

namespace Common
{
    public class EventProcessorFactory : IEventProcessorFactory
    {
        private readonly ConcurrentDictionary<string, EventHostProcessor> _eventProcessors = new ConcurrentDictionary<string, EventHostProcessor>();
        private readonly ConcurrentQueue<EventHostProcessor> _closedProcessors = new ConcurrentQueue<EventHostProcessor>();

        public event Action<string> OnMessageAction;
        public event Action<Exception> OnError;

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            var processor = new EventHostProcessor();            
            processor.ProcessorClosed += ProcessorOnProcessorClosed;
            processor.OnMessage += ProcessorOnMessage;
            processor.OnError += ProcessorOnError;

            _eventProcessors.TryAdd(context.Lease.PartitionId, processor);
            return processor;
        }

        private void ProcessorOnError(object sender, EventArgs e)
        {
            var args = e as HubErrorEventArgs;
            if (args != null)
            {
                this.OnError?.Invoke(args.Error);
            }
        }

        private void ProcessorOnMessage(object sender, EventArgs e)
        {
            var args = e as HubMessageEventArgs;
            if (args != null)
            {
                this.OnMessageAction?.Invoke(args.Message);
            }
        }

        public void ProcessorOnProcessorClosed(object sender, EventArgs eventArgs)
        {
            var processor = sender as EventHostProcessor;
            if (processor != null)
            {
                _eventProcessors.TryRemove(processor.Context.Lease.PartitionId, out processor);
                _closedProcessors.Enqueue(processor);
            }
        }
    }
}
