using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using System;
using System.Fabric;
using System.Threading.Tasks;

namespace ProxyService.Controllers.api
{
    [Route("api/[controller]")]
    public class ServiceBusController : Controller
    {

        private readonly FabricClient _client;
        private readonly IReliableStateManager _manager;
        private readonly StatefulServiceContext _context;
        private readonly ServicePartitionResolver servicePartitionResolver = ServicePartitionResolver.GetDefault();

        public ServiceBusController(IReliableStateManager manager, FabricClient fabricClient, StatefulServiceContext context)
        {
            _manager = manager;
            _client = fabricClient;
            _context = context;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Start(string id)
        {
            //var connString = @"Endpoint=sb://sfperf.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SeyAnVX09m8LZyvdXioxg9SElIpQBl882uz6DBBbBMo=";          
            //var sbSender = new ServiceBusTopicSender(connString, "service2", (e) =>
            //{

            //    ServiceEventSource.Current.ServiceMessage(_context, e.Message);
            //});

            //try
            //{                            
            //    var message = new ServiceMessage();
            //    message.CommChannel = "PubSub";
            //    message.SessionId = id;
            //    message.StampOne.Visited = true;
            //    message.StampOne.TimeNow = DateTime.UtcNow;
            //    sbSender.SendServiceMessageAsync(message).GetAwaiter().GetResult();

            //    return Ok(new { id = id });
            //}
            //catch(Exception e)
            //{
            //    return BadRequest(e.Message);
            //}

            //finally
            //{
            //    sbSender.Close();
            //}

            var message = new ServiceMessage();
            message.CommChannel = "PubSub";
            message.SessionId = id;
            message.StampOne.Visited = true;
            message.StampOne.TimeNow = DateTime.UtcNow;

            //var storage = await _manager.GetOrAddAsync<IReliableDictionary<string, ServiceMessage>>("storage");
            //using (var tx = _manager.CreateTransaction())
            //{
            //    await storage.AddAsync(tx, message.MessageId, message);
            //    await tx.CommitAsync();
            //}

            ServiceBusSenderClient.Send("service2", message, (e) => { ServiceEventSource.Current.ServiceMessage(_context, e.Message); });
            return Ok(new { id = id });
        }
    }
}
