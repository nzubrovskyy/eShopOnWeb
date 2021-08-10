using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DeliveryOrderProcessor
{
    public static class DeliveryOrderProcessor
    {
        [FunctionName("DeliverOrder")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "eshoponweb-orders",
                collectionName: "orders",
                ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<dynamic> documentsOut,
            ILogger log)
        {
            string order = await new StreamReader(req.Body).ReadToEndAsync();

            await documentsOut.AddAsync(order);

            log.LogInformation(order);

            return new OkObjectResult(order);
        }
    }
}
