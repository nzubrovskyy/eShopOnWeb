using Azure.Messaging.ServiceBus;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Data.Config;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.Infrastructure.Services
{
    public class EventBusAzureServiceBus<T> : IEventBus<T> where T : BaseEntity, IAggregateRoot
    {
        private readonly string _connectionString;
        private readonly string _queueName;

        public EventBusAzureServiceBus(IOptions<AzureServiceBusConfiguration> serviceBusOptions)
        {
            _connectionString = serviceBusOptions.Value.ConnectionString;
            _queueName = serviceBusOptions.Value.QueueName;
        }

        public async Task SendMessage(T entity)
        {
            await using (var client = new ServiceBusClient(_connectionString))
            {
                var sender = client.CreateSender(_queueName);

                var json = JsonConvert.SerializeObject(entity);
                var message = new ServiceBusMessage(json);

                await sender.SendMessageAsync(message);
            }
        }
    }
}
