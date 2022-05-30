using System;
using System.Threading.Tasks;
using AceSmokeShop.Core.IConfiguration;
using AceSmokeShop.Core.IRepositories;
using AceSmokeShop.Core.Repositories;
using AceSmokeShop.Data;
using AceSmokeShop.Repository.IRepositories;
using Microsoft.Extensions.Logging;

namespace AceSmokeShop.Areas.Identity.Data
{
    public class UnitOFWork : IUnitOfWork, IDisposable
    {
        private readonly DBContext _context;

        private readonly ILogger _logger;

        public IProductRepository Product { get; private set; }

        public ICategoryRepository Category { get; private set; }

        public UnitOFWork(DBContext context,
            ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger("logs");

            Product = new ProductRepository(_context, _logger);

            Category = new CategoryRepository(_context, _logger);
        }

        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
           _context.Dispose();
        }

        public object Where(Func<object, bool> p)
        {
            throw new NotImplementedException();
        }
    }
}
