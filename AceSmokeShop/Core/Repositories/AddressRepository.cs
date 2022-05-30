using AceSmokeShop.Core.IRepositories;
using AceSmokeShop.Data;
using AceSmokeShop.Models;
using Microsoft.Extensions.Logging;
namespace AceSmokeShop.Core.Repositories
{
    public class AddressRepository : GenericRepository<Addresses>, IAddressRepository
    {
        public AddressRepository(DBContext context,
            ILogger logger) : base(context, logger)
        {
        }
    }
}
