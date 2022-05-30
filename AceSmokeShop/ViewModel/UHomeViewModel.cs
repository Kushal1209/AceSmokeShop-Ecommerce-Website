using AceSmokeShop.Models;
using System.Collections.Generic;

namespace AceSmokeShop.ViewModel
{
    public class UHomeViewModel
    {
        public UHomeViewModel()
        {
            FeaturedList = new List<Product>();
            PromotedList = new List<Product>();
            CategoryList = new List<Category>();
        }

        public List<Product> FeaturedList { get; set; }
        public List<Product> PromotedList { get; set; }
        public List<Category> CategoryList { get; set; }
    }
}
