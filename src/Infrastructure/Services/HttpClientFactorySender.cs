using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Entities;

namespace Microsoft.eShopWeb.Infrastructure.Services
{
    public class HttpClientFactorySender<T>  where T : BaseEntity, IAggregateRoot
    {
    }
}
