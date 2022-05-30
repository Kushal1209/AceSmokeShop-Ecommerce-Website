using AceSmokeShop.Core.IRepositories;
using AceSmokeShop.Data;
using AceSmokeShop.Models;
using Microsoft.Extensions.Logging;

namespace AceSmokeShop.Core.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(DBContext context, 
            ILogger logger) : base(context, logger)
        {
        }
    }
}
