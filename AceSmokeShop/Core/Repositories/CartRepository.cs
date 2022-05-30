using AceSmokeShop.Core.IRepositories;
using AceSmokeShop.Data;
using AceSmokeShop.Models;
using AceSmokeShop.Repository.IRepositories;
using Microsoft.Extensions.Logging;
namespace AceSmokeShop.Core.Repositories
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        public CartRepository(DBContext context,
            ILogger logger) : base(context, logger)
        {
        }
    }
}

