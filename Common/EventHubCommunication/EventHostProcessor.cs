using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class EventHostProcessor : IEventProcessor
    {
        public event EventHandler ProcessorClosed;
        public event EventHandler OnMessage;
        public event EventHandler OnError;
        public PartitionContext Context { get; set; }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            /*
            Console.WriteLine(context.ConsumerGroupName);
            Console.WriteLine(context.EventHubPath);
            Console.WriteLine(context.Lease.SequenceNumber);
            */

            ProcessorClosed?.Invoke(this, null);

            return Task.FromResult(0);
        }

        public Task OpenAsync(PartitionContext context)
        {
            this.Context = context;
            return Task.FromResult(0);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            string myOffset;
            foreach (var message in messages)
            {
                myOffset = message.Offset;
                string body = Encoding.UTF8.GetString(message.GetBytes());
                OnMessage?.Invoke(this, new HubMessageEventArgs { Message = body });
            }
            try
            {
                await context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new HubErrorEventArgs { Error = ex });
            }
        }
    }
}
