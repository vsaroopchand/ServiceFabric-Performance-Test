using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Fabric;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Client;
using Common;
using Common.Grpc.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using System.Threading;
using Common.Grpc;

namespace ProxyService.Controllers.api
{
    [Route("api/[controller]")]
    public class GrpcController : Controller
    {
        private readonly FabricClient _client;
        private readonly IReliableStateManager _manager;
        private readonly StatefulServiceContext _context;
        private readonly ServicePartitionResolver servicePartitionResolver = ServicePartitionResolver.GetDefault();

        public GrpcController(IReliableStateManager manager, FabricClient fabricClient, StatefulServiceContext context)
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
                var message = new ServiceMessage2();
                message.CommChannel = "Grpc";
                message.SessionId = id;
                message.MessageId = Guid.NewGuid().ToString();
                message.StampOne = new Common.Grpc.VisitStamp();
                message.StampOne.Visited = true;
                message.StampOne.TimeNow = DateTime.UtcNow.Ticks;

                // initiatilize all timestamps since we dont have defaults on the protobuf generated objects
                message.StampTwo = new Common.Grpc.VisitStamp();
                message.StampTwo.Visited = false;
                message.StampTwo.TimeNow = 0;

                message.StampThree = new Common.Grpc.VisitStamp();
                message.StampThree.Visited = false;
                message.StampThree.TimeNow = 0;

                message.StampFour = new Common.Grpc.VisitStamp();
                message.StampFour.Visited = false;
                message.StampFour.TimeNow = 0;

                message.StampFive = new Common.Grpc.VisitStamp();
                message.StampFive.Visited = false;
                message.StampFive.TimeNow = 0;

                /*
                  var resolver = ServicePartitionResolver.GetDefault();
                  var serviceUri = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/Service2");
                  var communicationFactory = new GrpcCommunicationClientFactory<Common.Grpc.GrpcMessageService.GrpcMessageServiceClient>(null, resolver);          
                  var partitionList = await this._client.QueryManager.GetPartitionListAsync(serviceUri);

                  foreach (var partition in partitionList)
                  {
                      long partitionKey = ((Int64RangePartitionInformation)partition.PartitionInformation).HighKey;
                      var partitionClient = new ServicePartitionClient<GrpcCommunicationClient<Common.Grpc.GrpcMessageService.GrpcMessageServiceClient>>(communicationFactory, serviceUri, new ServicePartitionKey(partitionKey), listenerName: "grpc");
                      var reply = partitionClient.InvokeWithRetry((communicationClient) => communicationClient.Client.Send(message));
                  }
                  */

                var response = await GrpcClienHelper.SendMessage("Service2", message);


                return Ok(new { id = id });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
