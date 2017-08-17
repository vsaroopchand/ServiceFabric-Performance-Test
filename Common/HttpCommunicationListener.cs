using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Fabric;
using System.Fabric.Description;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public class WsCommunicationListener : ICommunicationListener
    {
        private readonly Action<byte[], CancellationToken, Action<byte[]>> serviceCallbackDelegate;
        private readonly string listeningAddress, publicAddress;
        private WebSocketListener webSocketListener;

        public WsCommunicationListener(StatelessServiceContext args, string endpointName, string appName, Action<byte[], CancellationToken, Action<byte[]>> callbackHandle)
        {
            this.serviceCallbackDelegate = callbackHandle;
            EndpointResourceDescription endpointDesc = args.CodePackageActivationContext.GetEndpoint(endpointName);

            appName = appName.Trim();
            if (!appName.EndsWith("/"))
            {
                appName += "/";
            }

            listeningAddress = $"http://+:{endpointDesc.Port}/{appName}";

            publicAddress = listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
            publicAddress = this.publicAddress.Replace("http", "ws");
        }
        public WsCommunicationListener(StatefulServiceContext args, string endpointName, string appName, Action<byte[], CancellationToken, Action<byte[]>> callbackHandle)
        {
            this.serviceCallbackDelegate = callbackHandle;

            EndpointResourceDescription endpointDesc = args.CodePackageActivationContext.GetEndpoint(endpointName);

            appName = appName.Trim();
            if (!appName.EndsWith("/"))
            {
                appName += "/";
            }

            listeningAddress = $"http://+:{endpointDesc.Port}/{appName}";

            publicAddress = listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
            publicAddress = this.publicAddress.Replace("http", "ws");
        }

        public void Abort()
        {
            this.webSocketListener.Stop(force: true);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            this.webSocketListener.Stop();

            return Task.FromResult(string.Empty);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            this.webSocketListener = new WebSocketListener(this.listeningAddress, this.serviceCallbackDelegate);
            this.webSocketListener.Start();

            return Task.FromResult(this.publicAddress);
        }
    }
    public class WebSocketListener : IDisposable
    {
        // https://github.com/paulbatum/WebSocket-Samples
        private readonly string prefix;
        private HttpListener httpListener;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;
        private readonly Action<byte[], CancellationToken, Action<byte[]>> callbackHandle;
        Task backgroundTask;

        public WebSocketListener(string address, Action<byte[], CancellationToken, Action<byte[]>> handle)
        {
            if (handle == null)
            {
                throw new ArgumentException($"{nameof(handle)} parameter is null.");
            }
            this.callbackHandle = handle;

            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentException($"WS address is invalid");
            }

            if (!address.EndsWith("/"))
            {
                address += "/";
            }

            this.prefix = address;

        }

        public void Start()
        {
            this.httpListener = new HttpListener();
            this.cancellationTokenSource = new CancellationTokenSource();
            this.cancellationToken = cancellationTokenSource.Token;

            this.httpListener.Prefixes.Add(prefix);
            this.httpListener.Start();
            this.backgroundTask = this.Listen();
        }

        private async Task Listen()
        {
            while (true)
            {
                var requestContext = await httpListener.GetContextAsync();
                if (requestContext.Request.IsWebSocketRequest)
                {
                    await ProcessConnectionAsync(requestContext);
                }
                else
                {
                    requestContext.Response.StatusCode = 400;
                    requestContext.Response.Close();
                }
            }
        }

        private async Task<bool> ProcessConnectionAsync(HttpListenerContext httpContext)
        {
            WebSocketContext webSocketContext = null;
            try
            {
                webSocketContext = await httpContext.AcceptWebSocketAsync(null);
            }
            catch (Exception ex)
            {
                SendError(httpContext, ex);
                return false;
            }

            WebSocket webSocket = webSocketContext.WebSocket;            
            try
            {

                byte[] receiveBuffer = new byte[102400];
                Action<byte[]> responseAction = (b) =>
                {
                    if (webSocket.State == WebSocketState.Open)
                    {
                        webSocket.SendAsync(
                            new ArraySegment<byte>(b),
                            WebSocketMessageType.Text,
                            true,
                            cancellationToken);
                    }

                };

                while (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), this.cancellationToken);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    }
                    //else if (receiveResult.MessageType == WebSocketMessageType.Text)
                    //{
                    //    await webSocket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Cannot accept text frame", CancellationToken.None);
                    //}
                    else if(receiveResult.EndOfMessage)
                    {      
                         callbackHandle(receiveBuffer, cancellationToken, responseAction);
                    }                                   
                }
        
            }
            catch (Exception ex)
            {
                SendError(httpContext, ex);
                return false;
            }

            return true;
        }

        private void SendError(HttpListenerContext httpContext, Exception e)
        {
            SendError(httpContext, e.Message);
        }

        private void SendError(HttpListenerContext httpContext, string message)
        {
            byte[] msgBytes = Encoding.Default.GetBytes(message);
            httpContext.Response.ContentLength64 = msgBytes.Length;
            httpContext.Response.StatusCode = 500;
            httpContext.Response.OutputStream.Write(msgBytes, 0, msgBytes.Length);
            httpContext.Response.OutputStream.Close();
            httpContext.Response.Close();
        }

        public void Stop(bool force = false)
        {
            if (backgroundTask != null)
            {
                backgroundTask.Dispose();
                backgroundTask = null;
            }


            if (this.httpListener != null && this.httpListener.IsListening)
            {
                if (force)
                {
                    this.httpListener.Abort();
                }
                else
                {
                    this.httpListener.Stop();
                    this.httpListener.Close();
                }

            }
        }

        public void Dispose()
        {
            try
            {
                if (this.cancellationTokenSource != null && !this.cancellationTokenSource.IsCancellationRequested)
                    this.cancellationTokenSource.Cancel();

                this.Stop();

                if (this.cancellationTokenSource != null && !this.cancellationTokenSource.IsCancellationRequested)
                    this.cancellationTokenSource.Dispose();
            }
            catch
            {
                httpListener.Abort();
                httpListener = null;
            }
        }
    }
}
