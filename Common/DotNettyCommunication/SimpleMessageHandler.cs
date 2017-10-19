using DotNetty.Transport.Channels;
using System;

namespace Common.DotNettyCommunication
{
    internal class SimpleMessageHandler : ChannelHandlerAdapter
    {
        private readonly Action<object> messageHandler;
        private readonly Action<Exception> errorHandler;

        public SimpleMessageHandler(Action<object> messageHandler, Action<Exception> errorHandler)
        {
            this.messageHandler = messageHandler;
            this.errorHandler = errorHandler;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            base.ChannelRead(context, message);
            if(message != null)
            {
                this.messageHandler?.Invoke(message);
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            this.errorHandler?.Invoke(exception);
            context.CloseAsync();
        }
    }
}