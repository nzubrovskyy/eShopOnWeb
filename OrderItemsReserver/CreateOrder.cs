using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace OrderItemsReserver
{
    public static class CreateOrder
    {
        /* https://laurakokkarinen.com/how-to-securely-call-a-logic-app-or-a-flow-from-an-azure-function-benefits/ 
         Queue message is posted into Logic App using Http trigger to prevent polling in Logic App in order to save money on account
         */
        [FunctionName("StoreOrder")]
        public static async Task Run(
            [ServiceBusTrigger("sbq-orders", Connection = "sb-eshoponweb_RootManageSharedAccessKey_SERVICEBUS")]string myQueueItem,
            //[Blob("orders/{rand-guid}.json", FileAccess.ReadWrite, Connection = "steshoponweb")] CloudBlockBlob outputBlob,
            ILogger log)
        {
            try
            {
                var logicAppUrl = GetAppSetting("orderItemsReserverLogicAppPostUrl");
                PostMessage(logicAppUrl, myQueueItem);

                //log.LogTrace(myQueueItem);
                //await outputBlob.UploadTextAsync(myQueueItem);
            }
            catch (Exception e)
            {
                log.Log(LogLevel.Error, $"{e.Message} {e.StackTrace}");
                throw; // The application needs to fail for the message to be retried or to end up in poison queue
            }            
        }

        public static void PostMessage(string url, string body)
        {
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                httpClient.DefaultRequestHeaders.Add("file-name", Guid.NewGuid().ToString());
                var response = httpClient.PostAsync(url, content).Result;
            }
        }
        public static string GetAppSetting(string key)
        {
            return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        }
    }
}
