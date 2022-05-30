using AceSmokeShop.Data;
using AceSmokeShop.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AceSmokeShop.Core.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        public DBContext _context;
        public DbSet<T> _dbSet;
        protected readonly ILogger _logger;

        public GenericRepository(DBContext context,
            ILogger logger)
        {
            _context = context;
            _logger = logger;
            _dbSet = context.Set<T>();

        }
    }
}
