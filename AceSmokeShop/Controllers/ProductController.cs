using Microsoft.AspNetCore.Mvc;
using AceSmokeShop.Areas.Identity.Data;
using AceSmokeShop.Data;
using AceSmokeShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using AceSmokeShop.Core.IConfiguration;
using AceSmokeShop.Core.Repositories;
using System.Linq;
using AceSmokeShop.ViewModel;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using AceSmokeShop.Services;
using Microsoft.AspNetCore.Http;
using System.IO;
using OfficeOpenXml;

namespace AceSmokeShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly DBContext _context;
        private AdminProductViewModel _productViewModel;
        private readonly AdminServices _adminServices;

        public ProductController(ILogger<ProductController> logger,
            IUnitOfWork unitOfWork, UserManager<AppUser> userManager,
            DBContext context)
        {
            _userManager = userManager;
            _context = context;
            _productViewModel = new AdminProductViewModel(); 
            _adminServices = new AdminServices(new ProductRepository(context, logger),
                new CategoryRepository(context, logger), new SubCategoryRepository(context, logger),
                new StateRepository(context, logger), userManager);
        }

        [HttpGet]
        public async Task<IActionResult> Product(int CategoryId = 0, int SubCategoryId = 0, int Min = 0,
                                                 int Max = 100000, string search = "", int sortBy = 0,
                                                 int sortByOrder = 0, int pageFrom = 1, int pageTotal = 10)
        {
            var user = await _userManager.GetUserAsync(User);

            ModelState.Clear();

            if (user.UserRole != null && user.UserRole == "ADMIN")
            {
                _productViewModel = await _adminServices.GetAdminProductViewModelAsync(user, CategoryId, SubCategoryId, Min, Max, search, sortBy, sortByOrder, pageFrom, pageTotal);
                _productViewModel.CategoryList.Insert(0, new Category { CategoryID = 0, CategoryName = "Select Category" });
                _productViewModel.SortByID = sortBy;
                _productViewModel.SortByOrder = sortByOrder;
                _productViewModel.SubCategoryList = new List<SubCategory>();
                if (CategoryId > 0)
                {
                    _productViewModel.SubCategoryList.AddRange(await _adminServices.GetSubCategoryListAsync(CategoryId));
                    _productViewModel.SubCategoryList.Insert(0, new SubCategory { SubCategoryID = 0, SubCategoryName = "Select SubCategory" });
                    if(SubCategoryId > 0)
                    {
                        _productViewModel.SubCategorySelect = _productViewModel.SubCategoryList.Where(x => x.SubCategoryID == SubCategoryId).FirstOrDefault();
                    }
                }
                
                _productViewModel.CategoryID = CategoryId;
                _productViewModel.CategorySelect = _productViewModel.CategoryList.Where(x => x.CategoryID == CategoryId).FirstOrDefault();
                _productViewModel.MinPrice = Min;
                _productViewModel.MaxPrice = Max;

                //if text search 
                if(search != null && search.Length >= 1)
                {
                    _productViewModel.Search = search;
                    _productViewModel.CategoryID = 0;
                    _productViewModel.SubCategoryID = 0;
                    _productViewModel.MinPrice = 0;
                    _productViewModel.MaxPrice = 100000;
                    _productViewModel.CategorySelect = new Category();
                    _productViewModel.SubCategoryName = "";
                    _productViewModel.SubCategoryList = new List<SubCategory>();
                }
                else if (CategoryId > 0 || SubCategoryId > 0 || Min > 0 || Max < 100000)
                {
                    _productViewModel.Search = "";
                }                    
                return View("Product", _productViewModel);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult UploadDataGet()
        {
            return View("UploadData");
        }

        public async Task<IActionResult> UploadData(IFormFile file)
        {
            var user = await _userManager.GetUserAsync(User);
            var list = new List<Product>();
            if (user == null || user.UserRole != "ADMIN")
            {
                return View("UploadData"); ;
            }
            else
            {
                list = await _adminServices.UploadProductSheetAsync(file);
                return View("UploadData");
            }
        }

        public async Task<IActionResult> ChangeFeature(string barcode = "")
        {
            var user = await _userManager.GetUserAsync(User);

            var result = await _adminServices.ChangeProductFeature(user, barcode);
            if (result.ToLower().Equals("success"))
            {
                return View("Product",_productViewModel);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        public async Task<IActionResult> ChangePromotion(string barcode = "")
        {
            var user = await _userManager.GetUserAsync(User);

            var result = await _adminServices.ChangeProductPromotion(user, barcode);
            if (result.ToLower().Equals("success"))
            {
                return View("Product", _productViewModel);
            }
            else
            {
                return StatusCode(500, result);
            }
        }


        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View("CreateCategory");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(AdminProductViewModel adminProductViewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if(user != null && user.UserRole == "ADMIN")
            {
                await _adminServices.addCategoryAsync(user, adminProductViewModel.newcategory);
            }

            return View(_productViewModel);

        }

        [HttpGet]
        public async Task<JsonResult> GetSubCatList(int CatID = 0)
        {
            List<SubCategory> subcat = new List<SubCategory>();

            subcat.AddRange(await _adminServices.GetSubCategoryListAsync(CatID));
            subcat.Insert(0, new SubCategory { SubCategoryID = 0, SubCategoryName = "Select SubCategory"});

            return Json(new SelectList(subcat, "SubCategoryID", "SubCategoryName"));
        }

        [HttpGet]
        public async Task<IActionResult> CreateSubCategory()
        {
            var user = await _userManager.GetUserAsync(User);
            _productViewModel = await _adminServices.GetAdminProductViewModelAsync(user, 0, 0, 0, 100000, "", 0, 1, 1, 20);
            _productViewModel.CategoryList.Insert(0, new Category { CategoryID = 0, CategoryName = "Select Category" });

            return View("CreateSubCategory", _productViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubCategory(AdminProductViewModel adminProductViewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if(user != null && user.UserRole == "ADMIN")
            {
                 await _adminServices.addSubCategoryAsync(user, adminProductViewModel.newsubcategory);
            }

            return View("Product", _productViewModel);

        }


        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            var user = await _userManager.GetUserAsync(User);
            _productViewModel = await _adminServices.GetAdminProductViewModelAsync(user, 0, 0, 0, 100000, "", 0, 1, 1, 20);
            _productViewModel.CategoryList.Insert(0, new Category { CategoryID = 0, CategoryName = "Select Category" });
            return View("CreateProduct",_productViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(AdminProductViewModel adminProductViewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            adminProductViewModel.newProduct.PrimaryImage = "https://thecigaretteboxes.com/public/assets/images/products/custom-paper-cigarette-boxes-frnt.png";
            var result = await _adminServices.addProductAsync(user, adminProductViewModel.newProduct);
            if (result.ToLower().Equals("success")) 
            {
                return View(adminProductViewModel);
            }
            else
            {
                return StatusCode(500, result);
            }

        }


        [HttpGet]
        public IActionResult EditProductGet(Product product)
        {
            return View("EditProduct", product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(Product product)
        {
            var user = await _userManager.GetUserAsync(User);

            var result = await _adminServices.editProductAsync(user, product);

            if (result.ToLower().Equals("success"))
            {
                return View(product);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpGet]
        public IActionResult DeleteProductGet(Product product)
        {

            return View("DeleteProduct", product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(Product product)
        {
            var user = await _userManager.GetUserAsync(User);

            var result = await _adminServices.deleteProduct(user, product.ProductID);

            if (result.ToLower().Equals("success"))
            {
                return View(product);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

    }
}
