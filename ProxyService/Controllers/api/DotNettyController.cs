using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Client;
using System;
using System.Fabric;
using System.Threading.Tasks;

namespace ProxyService.Controllers.api
{
    [Route("api/[controller]")]
    public class DotNettyController : Controller
    {

        private readonly FabricClient _client;
        private readonly IReliableStateManager _manager;
        private readonly StatefulServiceContext _context;
        private readonly ServicePartitionResolver servicePartitionResolver = ServicePartitionResolver.GetDefault();

        public DotNettyController(IReliableStateManager manager, FabricClient fabricClient, StatefulServiceContext context)
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
                var message = new ServiceMessage();
                message.CommChannel = "DotNettyTcp";
                message.SessionId = id;
                message.StampOne.Visited = true;
                message.StampOne.TimeNow = DateTime.UtcNow;
                Exception e = null;

                await Common.DotNettyCommunication.SimpleClient.SendAsync(message, this._context, "Service2", (err) => {
                    e = err;
                });

                if(e != null)
                {
                    throw e;
                }

                return Ok(new { id = id });
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }       
    }
}
