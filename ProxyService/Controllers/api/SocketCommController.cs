using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json;
using System;
using System.Fabric;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyService.Controllers.api
{
    [Route("api/[controller]")]
    public class SocketCommController : Controller
    {
        private readonly FabricClient _client;
        private readonly IReliableStateManager _manager;
        private readonly StatefulServiceContext _context;
        private readonly ServicePartitionResolver servicePartitionResolver = ServicePartitionResolver.GetDefault();

        public SocketCommController(IReliableStateManager manager, FabricClient fabricClient, StatefulServiceContext context)
        {
            _manager = manager;
            _client = fabricClient;
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Start(string id)
        {
            ClientWebSocket cws = new ClientWebSocket();
            byte[] receiveBuffer = new byte[102400];

            try
            {
                var endpoint = await Utils.GetSocketEndpoint("Service2", this._context);

                await cws.ConnectAsync(new Uri(endpoint), CancellationToken.None);
                if(cws.State == WebSocketState.Open)
                {
                    var message = new ServiceMessage();
                    message.CommChannel = "Socket";
                    message.SessionId = id;
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

            return Ok(new { id = id });
        }


        [HttpGet("endpoints/{id}")]
        public async Task<IActionResult> GetSocketEndpoint(string id)
        {
            try
            {                
                var address = await Utils.GetSocketEndpoint(id, this._context);

                return Ok(address);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}