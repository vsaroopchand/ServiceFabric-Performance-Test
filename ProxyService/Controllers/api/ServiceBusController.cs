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
            var message = new ServiceMessage();
            message.CommChannel = "PubSub";
            message.SessionId = id;
            message.StampOne.Visited = true;
            message.StampOne.TimeNow = DateTime.UtcNow;

            ConfigurationPackage configPackage = this._context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            ConfigurationSection configSection = configPackage.Settings.Sections[Constants.SB_CONFIG_SECTION];
            var connString = (configSection.Parameters[Constants.SB_CONN_STRING]).Value;
            var topicName = (configSection.Parameters[Constants.SB_TOPIC]).Value;

            await ServiceBusSenderClient2.Send(connString, topicName, message, (e) => { ServiceEventSource.Current.ServiceMessage(_context, e.Message); });

            return Ok(new { id = id });
        }
    }
}
