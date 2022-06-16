using AceSmokeShop.Core.IRepositories;
using AceSmokeShop.Data;
using AceSmokeShop.Models;
using Microsoft.Extensions.Logging;

namespace AceSmokeShop.Core.Repositories
{
    public class TransactionRepository : GenericRepository<Transactions>, ITransactionRepository
    {
        public TransactionRepository(DBContext context, ILogger logger) : base(context, logger)
        {
        }
    }
}
