using AceSmokeShop.Core.IRepositories;
using AceSmokeShop.Data;
using AceSmokeShop.Models;
using Microsoft.Extensions.Logging;

namespace AceSmokeShop.Core.Repositories
{
    public class OrderShipStatusRepository : GenericRepository<OrderShipStatus>, IOrderShipStatusRepository
    {
        public OrderShipStatusRepository(DBContext context, ILogger logger) : base(context, logger)
        {
        }
    }
}
