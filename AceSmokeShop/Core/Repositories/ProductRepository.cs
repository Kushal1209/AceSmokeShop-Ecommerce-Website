using AceSmokeShop.Data;
using AceSmokeShop.Models;
using AceSmokeShop.Repository.IRepositories;
using Microsoft.Extensions.Logging;
namespace AceSmokeShop.Core.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(DBContext context, 
            ILogger logger) : base(context, logger)
        {
        }
    }
}
