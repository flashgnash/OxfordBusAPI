using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using OxfordBusScraper;
using OxfordBusScraper.Models;

namespace OxfordBusAPI
{
    public class GetBusTimes
    {
        private readonly ILogger<GetBusTimes> _logger;

        public GetBusTimes(ILogger<GetBusTimes> log)
        {
            _logger = log;
        }

        [FunctionName("GetBusTimes")]
        [OpenApiOperation(operationId: "GetBusTimes", tags: new[] { "busStopId" })]
        [OpenApiParameter(name: "busStopId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Bus stop ID to get bus times of")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/json", bodyType: typeof(BusStopTimesResponse), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            try
            {
                _logger.LogInformation("C# HTTP trigger function processed a request. (GetBusTimes)");

                string busStopId = req.Query["busStopId"];

                if (busStopId == null || busStopId == "")
                    return new BadRequestErrorMessageResult($"Missing parameter {nameof(busStopId)}");

                OxfordBusClient client = new OxfordBusClient();

                try
                {
                    return new OkObjectResult(await client.GetBusTimesByStopAsync(busStopId));
                }
                catch (HttpRequestException e)
                {
                    if (e.StatusCode == HttpStatusCode.NotFound)
                        return new NotFoundResult();
                    else
                    {
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new InternalServerErrorResult();
            }

        }
    }
}

