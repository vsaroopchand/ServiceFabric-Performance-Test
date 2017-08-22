using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using ProxyService.Models;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyService.Controllers.api
{
    [Route("api/[controller]")]
    public class ResultsController : Controller
    {
        private readonly FabricClient _client;
        private readonly IReliableStateManager _manager;
        private readonly StatefulServiceContext _context;

        public ResultsController(IReliableStateManager manager, FabricClient fabricClient, StatefulServiceContext context)
        {
            _manager = manager;
            _client = fabricClient;
            _context = context;
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
    
            try
            {
                var results = await GetResultsAsync();
                var model = results.Select(t => { return new ResultModel().InitFromServiceMessage(t); });
                return Ok(model);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpGet("boxplot/{id}")]
        public async Task<IActionResult> BoxPlotResult(string id)
        {
            var dataPoints = new List<BoxPlotChartModel>();

            try
            {
                var results = await GetResultsAsync();
                var model = results
                                .Where(t => t.SessionId.Equals(id) && t.StampFive.Visited)
                                .Select(t => {
                                    return new ResultModel().InitFromServiceMessage(t);
                                });

                var lostPackets = results
                                .Where(t => t.SessionId.Equals(id) && !t.StampFive.Visited)
                                .Select(t => {
                                    return new ResultModel().InitFromServiceMessage(t);
                                }).ToList();

                if (lostPackets.Count > 0)
                {
                    // log the error
                    //ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");
                    ServiceEventSource.Current.ServiceMessage(_context, $"Losing message packets on {lostPackets.First().CommChannel} - {lostPackets.First().SessionId}");
                }

                var commGroups = model.GroupBy(t => t.CommChannel);
                foreach(IGrouping<string, ResultModel> group in commGroups)
                {
                    
                    var count = group.Count();                    
                    var averageStop1 = group.Average(t => t.Leg1.TotalMilliseconds);                    
                    var averageStop2 = group.Average(t => t.Leg2.TotalMilliseconds);                    
                    var averageStop3 = group.Average(t => t.Leg3.TotalMilliseconds);                    
                    var averageStop4 = group.Average(t => t.Leg4.TotalMilliseconds);                    
                    var totalTime = group.Average(t => t.TotalTravelTime.TotalMilliseconds);

                    var dp = new BoxPlotChartModel
                    {
                        label = group.Key,
                        values = new BoxPlotChartValues
                        {
                            Q1 = (decimal)averageStop1,
                            Q2 = (decimal)averageStop2,
                            Q3 = (decimal)averageStop3,
                            whisker_low = 0.0M,
                            whisker_high = (decimal)totalTime,
                            outliers = new List<decimal>
                            {
                                (decimal)averageStop1, (decimal)averageStop2, (decimal)averageStop3, (decimal)averageStop4, (decimal)totalTime
                            }
                        }
                    };
                    dataPoints.Add(dp);
                }

                //return Json(dataPoints, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                return Json(dataPoints);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private async Task<List<ServiceMessage>> GetResultsAsync()
        {
            var messages = new List<ServiceMessage>();
            var storage = await this._manager.GetOrAddAsync<IReliableDictionary<string, ServiceMessage>>("storage");
            using (var tx = _manager.CreateTransaction())
            {
                var messageEnumerable = await storage.CreateEnumerableAsync(tx, EnumerationMode.Ordered);
                using (Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<string, ServiceMessage>> enumerator = messageEnumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(CancellationToken.None))
                    {
                        messages.Add(enumerator.Current.Value);
                    }
                }
            }
            return messages;
        }
    }
}
