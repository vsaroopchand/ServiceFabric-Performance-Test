using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Newtonsoft.Json;
using System;
using System.Fabric;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Microsoft.ServiceFabric.Data.Collections;

namespace ProxyService.Controllers.api
{
    [Route("api/[controller]")]
    public class SocketCommController : Controller
    {
        private readonly FabricClient _client;
        private readonly IReliableStateManager _manager;
        private readonly StatefulServiceContext _context;

        public SocketCommController(IReliableStateManager manager, FabricClient fabricClient, StatefulServiceContext context)
        {
            _manager = manager;
            _client = fabricClient;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Start()
        {
            ClientWebSocket cws = new ClientWebSocket();
            byte[] receiveBuffer = new byte[102400];

            try
            {
                var endpoint = $"ws://localhost:{Constants.SVC2_WS_PORT}/Service2/";
                await cws.ConnectAsync(new Uri(endpoint), CancellationToken.None);
                if(cws.State == WebSocketState.Open)
                {
                    var message = new ServiceMessage();
                    message.CommChannel = "Socket";
                    message.StampOne.Visited = true;
                    message.StampOne.TimeNow = DateTime.UtcNow;
                    var messageJson = JsonConvert.SerializeObject(message);

                    var storage = await _manager.GetOrAddAsync<IReliableDictionary<string, ServiceMessage>>("storage");
                    using(var tx = _manager.CreateTransaction())
                    {
                        await storage.AddAsync(tx, message.MessageId, message);
                        await tx.CommitAsync();
                    }
                                      
                    Task receiverTask = cws.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                    Task sendTask = cws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(messageJson)), WebSocketMessageType.Binary, true, CancellationToken.None);

                    await Task.WhenAll(receiverTask, sendTask);

                }
            }
            finally
            {
                if (cws?.State == WebSocketState.Open || cws?.State == WebSocketState.Connecting)
                {
                    await cws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);                    
                }
            }

            return Ok();
        }

        /*

        [HttpGet]
        public IActionResult Start()
        {
            try
            {
                // https://github.com/sta/websocket-sharp
                // ws://localhost:4200/Service3/

                using (var ws = new WebSocket("ws://localhost:4200/Service3/"))
                {
                    ws.OnMessage += (sender, e) =>
                    {
                        if (e.IsBinary)
                        {
                            //Console.WriteLine("Binary package received from SVC");
                            var rawString = Encoding.UTF8.GetString(e.RawData);
                            //Console.WriteLine(rawString);
                        }
                        else if (e.IsText)
                        {
                            var responseMessage = JsonConvert.DeserializeObject<ServiceMessage>(e.Data);
                        }
                    };

                    ws.Connect();

                    ws.OnError += (sender, e) =>
                    {
                        //Console.WriteLine("ERROR ====>");
                        Console.WriteLine(e.Message);
                        //Console.WriteLine("<==== ERROR");
                    };
                    ws.OnOpen += (sender, e) =>
                    {
                        
                        Console.WriteLine("Connection opened");
                    };


                    var message = new ServiceMessage();
                    message.CommChannel = "Socket";
                    message.StampOne.Visited = true;
                    message.StampOne.TimeNow = DateTime.UtcNow;

                    var jsonPackage = JsonConvert.SerializeObject(message);
                    ws.Send(jsonPackage);

                    // keep communication open allowing Service Fabric to process the message reqeust
                    Thread.Sleep(200 * 100);
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        */
    }
}