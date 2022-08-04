using AceSmokeShop.Core.IRepositories;
using AceSmokeShop.Data;
using AceSmokeShop.Models;
using Microsoft.Extensions.Logging;

namespace AceSmokeShop.Core.Repositories
{
    public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(DBContext context, 
            ILogger logger) : base(context, logger)
        {
        }
    }
}
