using DotNetty.Codecs;
using DotNetty.Codecs.Base64;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.DotNettyCommunication
{
    public class Bootstrapper
    {
        readonly TaskCompletionSource closeCompletionSource;
        readonly string host;
        readonly int port;
        private readonly Action<object> messageHandle;
        private readonly Action<Exception> errorHandle;
        IEventLoopGroup boss;
        IEventLoopGroup worker;
        IChannel serverChannel;

        public Bootstrapper(string host, int port, Action<object> messageHandle, Action<Exception> errorHandle)
        {
            this.host = host;
            this.port = port;
            this.messageHandle = messageHandle;
            this.errorHandle = errorHandle;
            this.closeCompletionSource = new TaskCompletionSource();
        }

        public Task ClosedCompletion => this.closeCompletionSource.Task;

        public async Task RunAsync(int threadCount, CancellationToken cancellationToken)
        {
            this.boss = new MultithreadEventLoopGroup(1);
            this.worker = new MultithreadEventLoopGroup(threadCount);

            var serverBootstrap = SetupServerBootstrap();            
            this.serverChannel = await serverBootstrap.BindAsync(new IPEndPoint(IPAddress.Parse(this.host), this.port));
            
            this.serverChannel.CloseCompletion.LinkOutcome(this.closeCompletionSource);
            cancellationToken.Register(this.CloseAsync);
     
        }

        async void CloseAsync()
        {
            try
            {
  
                if (this.serverChannel != null)
                {
                    await this.serverChannel.CloseAsync();
                }
                if (this.worker != null)
                {
                    await this.worker.ShutdownGracefullyAsync();
                }
                
            }
            catch (Exception ex)
            {
                //
            }
            finally
            {
                this.closeCompletionSource.TryComplete();
            }
        }

        ServerBootstrap SetupServerBootstrap()
        {
            var bootstrap = new ServerBootstrap();
            bootstrap.Group(this.boss, this.worker)
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.SoBacklog, 100)
                .Handler(new LoggingHandler("TEST", LogLevel.TRACE))
                .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast(new LoggingHandler("CONN"));
                    pipeline.AddLast(new Base64Encoder(), new StringEncoder(Encoding.UTF8), new Base64Decoder(), new StringDecoder(Encoding.UTF8), new SimpleMessageHandler(this.messageHandle, this.errorHandle));                    
                }));

            return bootstrap;
        }
    }
}
