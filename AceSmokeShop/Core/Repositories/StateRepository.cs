using AceSmokeShop.Core.IRepositories;
using AceSmokeShop.Data;
using AceSmokeShop.Models;
using Microsoft.Extensions.Logging;


namespace AceSmokeShop.Core.Repositories
{
    public class StateRepository : GenericRepository<State>, IStateRepository
    {
        public StateRepository(DBContext context, 
            ILogger logger) : base(context, logger)
        {
        }
    }
}
