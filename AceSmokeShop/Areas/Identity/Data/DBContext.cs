using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AceSmokeShop.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AceSmokeShop.Data
{
    public class DBContext : IdentityDbContext<AppUser>
    {
        public DBContext()
        {
        }

        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        public DbSet<AceSmokeShop.Models.State> tbl_state { get; set; }

        public DbSet<AceSmokeShop.Models.Product> tbl_product { get; set; }

        public DbSet<AceSmokeShop.Models.Brand> tbl_brand { get; set; }

        public DbSet<AceSmokeShop.Models.Category> tbl_category { get; set; }

        public DbSet<AceSmokeShop.Models.SubCategory> tbl_subcategory { get; set; }

        public DbSet<AceSmokeShop.Models.Cart> tbl_cart { get; set; }
        public DbSet<AceSmokeShop.Models.Addresses> tbl_addresses { get; set; }
        public DbSet<AceSmokeShop.Models.UserOrders> tbl_userorders { get; set; }
        public DbSet<AceSmokeShop.Models.OrderItem> tbl_orderitem { get; set; }
        public DbSet<AceSmokeShop.Models.OrderShipStatus> tbl_ordershipstatus { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
