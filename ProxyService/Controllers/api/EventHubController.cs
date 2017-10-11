using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Client;
using System;
using System.Fabric;
using System.Fabric.Description;
using System.Threading.Tasks;

namespace ProxyService.Controllers.api
{

    [Route("api/[controller]")]
    public class EventHubController : Controller
    {
        private readonly FabricClient _client;
        private readonly IReliableStateManager _manager;
        private readonly StatefulServiceContext _context;
        private readonly ServicePartitionResolver servicePartitionResolver = ServicePartitionResolver.GetDefault();

        public EventHubController(IReliableStateManager manager, FabricClient fabricClient, StatefulServiceContext context)
        {
            _manager = manager;
            _client = fabricClient;
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Start(string id)
        {
            var message = new ServiceMessage();
            message.CommChannel = "EventHub";
            message.SessionId = id;
            message.StampOne.Visited = true;
            message.StampOne.TimeNow = DateTime.UtcNow;

            ConfigurationPackage configPackage = this._context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            ConfigurationSection configSection = configPackage.Settings.Sections[Constants.EH_CONFIG_SECTION];
            var connString = (configSection.Parameters[Constants.EH_CONN_STRING]).Value;
            var path = (configSection.Parameters[Constants.EH_SENDTO_HUB_PATH]).Value;
            
            try
            {                
                await new EventHubSender(connString, path).Send(message);
                return Ok(new { id = id });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}