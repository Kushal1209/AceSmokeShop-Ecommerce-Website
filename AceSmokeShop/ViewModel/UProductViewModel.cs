using AceSmokeShop.Models;
using System.Collections.Generic;

namespace AceSmokeShop.ViewModel
{
    public class UProductViewModel
    {
        public UProductViewModel()
        {
            ProdctList = new List<Product>();
            CategoryList = new List<Category>();
            SubCategoryList = new List<SubCategory>();

            CategorySelect = new Category();
            SubCategorySelect = new SubCategory();

            Search = "";
            ItemsPerPage = 2;
            CurrentPage = 1;
            TotalPages = 1;

            Type = "";

            ProductsPerPages = new List<int> { 2, 10, 30, 50, 100, 200 };

            IsFeatured = false;
            IsPromoted = false;
        }

        public List<Product> ProdctList { get; set; }
        public List<Category> CategoryList { get; set; }
        public List<SubCategory> SubCategoryList { get; set; }

        public string Type { get; set; }

        public Category CategorySelect { get; set; }
        public SubCategory SubCategorySelect { get; set; }

        public string Search { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public List<int> ProductsPerPages { get; set; }

        public bool IsFeatured { get; set; }
        public bool IsPromoted { get; set; }

    }
}
