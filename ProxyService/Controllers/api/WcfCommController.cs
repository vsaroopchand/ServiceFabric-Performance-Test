using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Fabric;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Client;
using Common;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Data.Collections;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProxyService.Controllers.api
{
    [Route("api/[controller]")]
    public class WcfCommController : Controller
    {
        private readonly FabricClient _client;
        private readonly IReliableStateManager _manager;
        private readonly StatefulServiceContext _context;
        private readonly string ServiceName = "Service2";

        public WcfCommController(IReliableStateManager manager, FabricClient fabricClient, StatefulServiceContext context)
        {
            _manager = manager;
            _client = fabricClient;
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Start(string id)
        {
            try
            {
                var serviceUri = this._context.CodePackageActivationContext.ApplicationName + "/" + ServiceName;
                var partitionList = await this._client.QueryManager.GetPartitionListAsync(new Uri(serviceUri));
                var bindings = WcfUtility.CreateTcpClientBinding();
                var partitionResolver = ServicePartitionResolver.GetDefault();
               
                var message = new ServiceMessage();
                message.CommChannel = "WcfNetTcp";
                message.SessionId = id;
                message.StampOne.Visited = true;
                message.StampOne.TimeNow = DateTime.UtcNow;
                var storage = await _manager.GetOrAddAsync<IReliableDictionary<string, ServiceMessage>>("storage");
                using (var tx = _manager.CreateTransaction())
                {
                    await storage.AddAsync(tx, message.MessageId, message);
                    await tx.CommitAsync();
                }

                foreach (var partition in partitionList)
                {                   
                    var partitionInfo = (Int64RangePartitionInformation)partition.PartitionInformation;
                    var wcfClientFactory = new WcfCommunicationClientFactory<IServiceTwo>(clientBinding: bindings, servicePartitionResolver: partitionResolver);
                    var wcfClient = new SvcTwoWcfCommunicationClient(wcfClientFactory, new Uri(serviceUri), new ServicePartitionKey(partitionInfo.HighKey), listenerName: "WcfTcp");
                    await wcfClient.InvokeWithRetryAsync(
                            client => client.Channel.VisitWcfAsync(message));

                }

                return Ok();
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }

}
