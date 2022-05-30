using AceSmokeShop.Core.IRepositories;
using AceSmokeShop.Data;
using AceSmokeShop.Models;
using Microsoft.Extensions.Logging;

namespace AceSmokeShop.Core.Repositories
{
    public class UserOrdersRepository : GenericRepository<UserOrders>, IUserOrdersRepository
    {
        public UserOrdersRepository(DBContext context, ILogger logger) : base(context, logger)
        {
        }
    }
}
