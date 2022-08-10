using AceSmokeShop.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace AceSmokeShop.ViewModel
{
    public class AdminProductViewModel
    {

        public AdminProductViewModel() {

            CategoryID = 0;
            CategorySelect = new Category();
            SubCategorySelect = new SubCategory();
            SubCategoryID = 0;
            SubCategoryName = "Select Sub-Category";

            SortByOrder = 1;
            SortByID = 0;
            Search = "";

            ProductList = new List<Product>();
            CategoryList = new List<Category>();
            SubCategoryList = new List<SubCategory>();

            MinPrice = 0;
            MaxPrice = 100000;
            TotalProducts = 0;

            RowPerPage = new List<int> { 10, 20, 50, 100 };
            CurrentPage = 1;
            TotalPages = 0;
            ItemsPerPage = 10;

            newProduct = new Product();
            editproduct = new Product();
            newcategory = new Category();
            newsubcategory = new SubCategory();
        }

        public Product editproduct { get; set; }
        public Category newcategory { get; set; }
        public SubCategory newsubcategory { get; set; }
        public Product newProduct { get; set; }
        public List<Product> ProductList { get; set; }
        public List<Category> CategoryList  { get; set; }
        public List<SubCategory> SubCategoryList  { get; set; }

        public int TotalProducts;
        public int MinPrice;
        public int MaxPrice;
        
        //Page Data
        public int CurrentPage;
        public int TotalPages;
        public int ItemsPerPage;

        public int CategoryID { get; set; }
        public Category CategorySelect { get; set; }

        public SubCategory SubCategorySelect { get; set; }

        public int SubCategoryID { get; set; }
        public string SubCategoryName { get; set; }

        public List<int> RowPerPage { get; set; }

        public string Search { get; set; }
        public int SortByOrder { get; set; }
        public int SortByID { get; set; }

    }
}
