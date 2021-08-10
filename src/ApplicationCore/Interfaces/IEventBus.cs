using Microsoft.eShopWeb.ApplicationCore.Entities;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces
{
    public interface IEventBus<T> where T : BaseEntity, IAggregateRoot
    {
        Task SendMessage(T entity);
    }
}