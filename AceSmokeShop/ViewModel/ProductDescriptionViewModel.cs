
using AceSmokeShop.Models;
using System.Collections.Generic;

namespace AceSmokeShop.ViewModel
{
    public class ProductDescriptionViewModel
    {
        public ProductDescriptionViewModel()
        {
            product = new Product();
            RecomendedProductList = new List<Product>();

            Qty = 1;
        }

        public Product product { get; set; }
        public List<Product> RecomendedProductList { get; set; }

        public int Qty { get; set; }
    }
}
