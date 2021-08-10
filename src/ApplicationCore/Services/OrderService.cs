using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Services
{
    public class OrderService : IOrderService
    {
        private readonly IAsyncRepository<Order> _orderRepository;
        private readonly IUriComposer _uriComposer;
        private readonly IAsyncRepository<Basket> _basketRepository;
        private readonly IAsyncRepository<CatalogItem> _itemRepository;
        private readonly IEventBus<Order> _eventBus;
        private readonly IHttpClientFactory _clientFactory;

        public OrderService(IAsyncRepository<Basket> basketRepository,
            IAsyncRepository<CatalogItem> itemRepository,
            IAsyncRepository<Order> orderRepository,
            IEventBus<Order> eventBus,
            IUriComposer uriComposer,
            IHttpClientFactory clientFactory)
        {
            _orderRepository = orderRepository;
            _uriComposer = uriComposer;
            _basketRepository = basketRepository;
            _itemRepository = itemRepository;
            _eventBus = eventBus;
            _clientFactory = clientFactory;
        }

        public async Task CreateOrderAsync(int basketId, Address shippingAddress)
        {
            var basketSpec = new BasketWithItemsSpecification(basketId);
            var basket = await _basketRepository.FirstOrDefaultAsync(basketSpec);

            Guard.Against.NullBasket(basketId, basket);
            Guard.Against.EmptyBasketOnCheckout(basket.Items);

            var catalogItemsSpecification = new CatalogItemsSpecification(basket.Items.Select(item => item.CatalogItemId).ToArray());
            var catalogItems = await _itemRepository.ListAsync(catalogItemsSpecification);

            var items = basket.Items.Select(basketItem =>
            {
                var catalogItem = catalogItems.First(c => c.Id == basketItem.CatalogItemId);
                var itemOrdered = new CatalogItemOrdered(catalogItem.Id, catalogItem.Name, _uriComposer.ComposePicUri(catalogItem.PictureUri));
                var orderItem = new OrderItem(itemOrdered, basketItem.UnitPrice, basketItem.Quantity);
                return orderItem;
            }).ToList();

            var order = new Order(basket.BuyerId, shippingAddress, items);

            await _orderRepository.AddAsync(order);

            await _eventBus.SendMessage(order);

            await DeliverOrder(order);
        }

        private async Task DeliverOrder(Order order)
        {
            string jsonString = JsonSerializer.Serialize(order);
            var payload = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var client = _clientFactory.CreateClient();
               
            HttpResponseMessage response = await client.PostAsync(
                Environment.GetEnvironmentVariable("baseUrls:deliveryOrderProcessorConnectionString"),
                payload);

            string responseJson = await response.Content.ReadAsStringAsync();
        }
    }
}
