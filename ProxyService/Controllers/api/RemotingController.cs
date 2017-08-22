using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyService.Controllers.api
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class RemotingController : Controller
    {
        private const string Endpoint = "fabric:/SfPerfTest/Service2";
        private IReliableStateManager _manager;
        private readonly FabricClient _client;
        private readonly StatefulServiceContext _context;
        private readonly ServicePartitionResolver servicePartitionResolver = ServicePartitionResolver.GetDefault();

        public RemotingController(IReliableStateManager manager, FabricClient fabricClient, StatefulServiceContext context)
        {
            _manager = manager;
            _client = fabricClient;
            _context = context;
        }


        [HttpGet("start/{id}")]
        public async Task<IActionResult> Start(string id)
        {
            try
            {
                var message = new ServiceMessage();
                message.CommChannel = "Remoting";
                message.SessionId = id;
                message.StampOne.Visited = true;
                message.StampOne.TimeNow = DateTime.UtcNow;

                //var storage = await _manager.GetOrAddAsync<IReliableDictionary<string, ServiceMessage>>("storage");
                //using (var tx = _manager.CreateTransaction())
                //{
                //    await storage.AddAsync(tx, message.MessageId, message);
                //    await tx.CommitAsync();
                //}

                var partitionKey = new ServicePartitionKey(1);
                var service = ServiceProxy.Create<IServiceTwo>(new Uri(Endpoint), partitionKey);
                await service.VisitByRemotingAsync(message);

                return Ok(new { id = id});
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [HttpPost("end")]
        public async Task<IActionResult> End([FromBody] ServiceMessage message)
        {

            try
            {
                message.StampFive.Visited = true;
                message.StampFive.TimeNow = DateTime.UtcNow;

                var storage = await _manager.GetOrAddAsync<IReliableDictionary<string, ServiceMessage>>("storage");
                using (var tx = _manager.CreateTransaction())
                {                    
                    await storage.AddOrUpdateAsync(tx, message.MessageId, message, (k, m) =>
                    {
                        //m.StampOne = message.StampOne;
                        //m.StampTwo = message.StampTwo;
                        //m.StampThree = message.StampThree;
                        //m.StampFour = message.StampFour;

                        //m.StampFive.Visited = true;
                        //m.StampFive.TimeNow = DateTime.UtcNow;
                        //return m;
                        return message;
                    });

                    await tx.CommitAsync();
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpGet("partition/{id}")]
        public async Task<IActionResult> GetServicePartitiion(string id)
        {
            try
            {
                List<string> partitionIds = new List<string>();
                var serviceUri = this._context.CodePackageActivationContext.ApplicationName + "/" + id;
                var partitionList = await this._client.QueryManager.GetPartitionListAsync(new Uri(serviceUri));

                foreach(var partition in partitionList)
                {
                    partitionIds.Add(partition.PartitionInformation.Id.ToString());

                    long partitionKey = ((Int64RangePartitionInformation)partition.PartitionInformation).HighKey;
                    var resolvedPartition = await servicePartitionResolver.ResolveAsync(new Uri(serviceUri), new ServicePartitionKey(partitionKey), CancellationToken.None);
                    var endpoint = resolvedPartition.GetEndpoint();
                    
                    
                    var message = new ServiceMessage();
                    message.CommChannel = "Remoting";
                    message.StampOne.Visited = true;
                    message.StampOne.TimeNow = DateTime.UtcNow;

                    var storage = await _manager.GetOrAddAsync<IReliableDictionary<string, ServiceMessage>>("storage");
                    using (var tx = _manager.CreateTransaction())
                    {
                        await storage.AddAsync(tx, message.MessageId, message);
                        await tx.CommitAsync();
                    }

                    
                    var service = ServiceProxy.Create<IServiceThree>(new Uri(serviceUri), new ServicePartitionKey(partitionKey));
                    await service.VisitByRemotingAsync(message);

                }

                return Ok(partitionIds);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpGet("endpoints/{id}")]
        public async Task<IActionResult> GetServiceEndpoints(string id)
        {
            try
            {
                Dictionary<string, ResolvedServiceEndpoint> partitionEndpointDictionary = new Dictionary<string, ResolvedServiceEndpoint>();
                List<string> endpoints = new List<string>();

                var serviceUri = this._context.CodePackageActivationContext.ApplicationName + "/" + id;
                var partitionList = await this._client.QueryManager.GetPartitionListAsync(new Uri(serviceUri));

                foreach (var partition in partitionList)
                {
                    long partitionKey = ((Int64RangePartitionInformation)partition.PartitionInformation).HighKey;
                    var resolvedPartition = await servicePartitionResolver.ResolveAsync(new Uri(serviceUri), new ServicePartitionKey(partitionKey), CancellationToken.None);
                    var endpoint = resolvedPartition.GetEndpoint();
                    
                    foreach(var ep in resolvedPartition.Endpoints)
                    {
                        endpoints.Add(ep.Address);
                    }

                    //partitionEndpointDictionary.Add(partition.PartitionInformation.Id.ToString(), endpoint);
                }              

                return Ok(endpoints);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}