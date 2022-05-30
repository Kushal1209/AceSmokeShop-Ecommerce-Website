using AceSmokeShop.Core.IRepositories;
using AceSmokeShop.Repository.IRepositories;
using System;
using System.Threading.Tasks;

namespace AceSmokeShop.Core.IConfiguration
{
    public interface IUnitOfWork
    {
        IProductRepository Product { get; }
        ICategoryRepository Category { get; }

        Task CompleteAsync();
        object Where(Func<object, bool> p);
    }
}
