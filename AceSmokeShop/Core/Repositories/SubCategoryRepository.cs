using AceSmokeShop.Core.IRepositories;
using AceSmokeShop.Data;
using AceSmokeShop.Models;
using Microsoft.Extensions.Logging;

namespace AceSmokeShop.Core.Repositories
{
    public class SubCategoryRepository : GenericRepository<SubCategory>, ISubCategoryRepository
    {
        public SubCategoryRepository(DBContext context, 
            ILogger logger) : base(context, logger)
        {
        }
    }
}
