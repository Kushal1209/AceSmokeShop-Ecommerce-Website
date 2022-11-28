using AceSmokeShop.Areas.Identity.Data;
using AceSmokeShop.Core.Repositories;
using AceSmokeShop.Data;
using AceSmokeShop.Models;
using AceSmokeShop.Services;
using AceSmokeShop.ViewModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AceSmokeShop.Controllers
{
    public class WebAPIController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AdminServices _adminServices;
        public PaymentServices _paymentServices;
        private AdminProductViewModel _productViewModel;

        public WebAPIController(ILogger<WebAPIController> logger, SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager, DBContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _paymentServices = new PaymentServices(new ProductRepository(context, logger),
                new CategoryRepository(context, logger), new SubCategoryRepository(context, logger),
                new StateRepository(context, logger), userManager, new CartRepository(context, logger),
                new AddressRepository(context, logger), new TransactionRepository(context, logger),
                new UserOrdersRepository(context, logger));
            _adminServices = new AdminServices(new ProductRepository(context, logger),
                new CategoryRepository(context, logger), new SubCategoryRepository(context, logger),
                new StateRepository(context, logger), userManager, new CartRepository(context, logger), new AddressRepository(context, logger), _paymentServices,
                new UserOrdersRepository(context, logger), new OrderItemRepository(context, logger), new TransactionRepository(context, logger));
        }


        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpGet]
        [BasicAuthorization]
        public async Task<string> SayHii()
        {
            var user = await GetUserAsync();
            return "Hiiiiiiiiiiiiiiiiiiiiiiiiiiii";
        }

        private async Task<AppUser> GetUserAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return null;
            }

            var authorizationheader = Request.Headers["Authorization"].ToString();
            var authHeaderRegex = new Regex("Basic (.*)");
            if (!authHeaderRegex.IsMatch(authorizationheader))
            {
                return null;
            }

            var authBase64 = Encoding.UTF8.GetString(Convert.FromBase64String(authHeaderRegex.Replace(authorizationheader, "$1")));

            var creds = authBase64.Split(':', 2);
            var authUsername = creds[0];
            var authPassword = creds.Length > 1 ? creds[1] : throw new Exception("Unable to get Password");

            var user = await LoginInternal(authUsername, authPassword);
            return user;
        }

        public async Task<JsonResult> LoginAsync(string Email, string Password)
        {
            var user = await LoginInternal(Email, Password);
 
            return Json(user);
        }

        private async Task<AppUser> LoginInternal(string Email, string Password)
        {
            var user = await _userManager.FindByEmailAsync(Email);

            if (user == null)
            {
                return null;
            }
            if (user.LockoutEnabled == true)
            {
                return null;
            }
            var result = await _signInManager.PasswordSignInAsync(Email, Password, false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return user;
            }
            if (result.IsLockedOut)
            {
                return null;
            }
            else
            {
                return null;
            }
        }

        [BasicAuthorization]
        public bool VerifyToken()
        {
            return true;
        }

        [HttpGet]
        [BasicAuthorization]
        public async Task<JsonResult> GetAdminDashboard(int pageFrom = 1, int pageTotal = 10, int period = 0,
                        string datefrom = "", string dateto = "")
        {
            var user = await GetUserAsync();
            if (user.UserRole.Equals("ADMIN")){
                DateTime DateFrom = DateTime.MinValue;
                DateTime DateTo = DateTime.MaxValue;
                try
                {
                    DateFrom = datefrom == null || datefrom == "" ? DateTime.MinValue : FormatDate(datefrom, true);
                    DateTo = dateto == null || dateto == "" ? DateTime.MaxValue : FormatDate(dateto, false);
                }
                catch(Exception ex)
                {
                    return Json(ex);
                }
                var model = _adminServices.GetAdminFinancialViewModel(user, pageFrom, pageTotal, period, DateFrom, DateTo);

                return Json(model);
            }
            else
            {
                return Json("");
            }
        }

        private DateTime FormatDate(string datefrom, bool ismin)
        {
            try
            {
                var strDate = datefrom.Split("/");
                var finalDate = strDate[1] + "-" + strDate[0] + "-" + strDate[2];
                DateTime dt = DateTime.Parse(finalDate);
                return dt;
            }
            catch (Exception ex)
            {
                return ismin ? DateTime.MinValue : DateTime.MaxValue;
            }

        }

        [HttpGet]
        [BasicAuthorization]
        public async Task<JsonResult> GetUserAccount(int StateID = 0, string search = "", string UserRole = "", int pageFrom = 1, int pageTotal = 10)
        {
            var user = await GetUserAsync();
            if (user.UserRole.Equals("ADMIN"))
            {
                var _adminUserViewModel = await _adminServices.GetAdminUserAccount(user, StateID, search, UserRole, pageFrom, pageTotal);
                _adminUserViewModel.StateList.Insert(0, new State { StateID = 0, StateName = "Select State" });
                _adminUserViewModel.StateID = StateID;
                _adminUserViewModel.UserRole = UserRole;
                _adminUserViewModel.Search = search;
                _adminUserViewModel.CurrentPage = pageFrom;
                _adminUserViewModel.ItemsPerPage = pageTotal;
                _adminUserViewModel.StateSelect = _adminUserViewModel.StateList.Where(x => x.StateID == StateID).FirstOrDefault();

                return Json(_adminUserViewModel);
            }
            else
            {
                return Json("");
            }
        }

        [HttpGet]
        [BasicAuthorization]
        public async Task<JsonResult> GetProduct(int CategoryId = 0, int SubCategoryId = 0, int Min = 0,
                                                 int Max = 100000, string search = "", int sortBy = 0,
                                                 int sortByOrder = 0, int pageFrom = 1, int pageTotal = 10)
        {
            var user = await GetUserAsync();

            ModelState.Clear();

            if (user.UserRole != null && user.UserRole == "ADMIN")
            {
                var _productViewModel = await _adminServices.GetAdminProductViewModelAsync(user, CategoryId, SubCategoryId, Min, Max, search, sortBy, sortByOrder, pageFrom, pageTotal);
                _productViewModel.CategoryList.Insert(0, new Category { CategoryID = 0, CategoryName = "Select Category" });
                _productViewModel.SortByID = sortBy;
                _productViewModel.SortByOrder = sortByOrder;
                _productViewModel.SubCategoryList = new List<SubCategory>();

                if (CategoryId > 0)
                {
                    _productViewModel.SubCategoryList.AddRange(await _adminServices.GetSubCategoryListAsync(CategoryId));
                    _productViewModel.SubCategoryList.Insert(0, new SubCategory { SubCategoryID = 0, SubCategoryName = "Select SubCategory" });
                    if (SubCategoryId > 0)
                    {
                        _productViewModel.SubCategorySelect = _productViewModel.SubCategoryList.Where(x => x.SubCategoryID == SubCategoryId).FirstOrDefault();
                    }
                }

                _productViewModel.CategoryID = CategoryId;
                _productViewModel.CategorySelect = _productViewModel.CategoryList.Where(x => x.CategoryID == CategoryId).FirstOrDefault();
                _productViewModel.MinPrice = Min;
                _productViewModel.MaxPrice = Max;

                //if text search 
                if (search != null && search.Length >= 1)
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
                return Json( _productViewModel);
            }
            else
            {
                return Json("");
            }
        }

        [HttpPost]
        [BasicAuthorization]
        public async Task<JsonResult> CreateCategory(string newcategory)
        {
            var user = await GetUserAsync();
            var category = new Category();
            category.CategoryName = newcategory;
            if (user != null && user.UserRole == "ADMIN")
            {
                await _adminServices.addCategoryAsync(user, category);
            }

            return Json(_productViewModel);
        }

        [HttpPost]
        [BasicAuthorization]
        public async Task<JsonResult> CreateSubCategory(int CatID, string SubCatName)
        {
            var user = await GetUserAsync();
            var newsubCat = new SubCategory();
            newsubCat.SubCategoryName = SubCatName;
            newsubCat.CategoryID = CatID;
            if (user != null && user.UserRole == "ADMIN")
            {
                await _adminServices.addSubCategoryAsync(user, newsubCat);
            }

            return Json(_productViewModel);
        }

        [HttpPost]
        [BasicAuthorization]
        public async Task<JsonResult> DeleteProduct(int productId)
        {
            var user = await GetUserAsync();

            var result = await _adminServices.deleteProduct(user, productId);

            if (result.ToLower().Equals("success"))
            {
                return Json(productId);
            }
            else
            {
                return Json(500, result);
            }
        }

        [HttpPost]
        [BasicAuthorization]
        public async Task<JsonResult> CreateProduct()
        {
            var user = await GetUserAsync();
            var req = HttpContext.Request;
            string reqBodyString;
            Product newProduct;
            try
            {
                req.EnableBuffering();
                var buffer = new byte[Convert.ToInt32(req.ContentLength)];
                await req.Body.ReadAsync(buffer, 0, buffer.Length);
                reqBodyString = Encoding.UTF8.GetString(buffer);
                newProduct = JsonConvert.DeserializeObject<Product>(reqBodyString);

            }
            finally
            {
                req.Body.Position = 0;
            }
            var result = await _adminServices.addProductAsync(user, newProduct);
            if (result.ToLower().Contains("success"))
            {
                int prodId = int.Parse(result.Replace("Success", ""));
                if (prodId > 0)
                    return Json(prodId);
                else
                    return Json("Fail");
            }
            else
            {
                return Json("Fail");
            }
        }

        [HttpPost]
        [BasicAuthorization]
        public async Task<JsonResult> UploadProdImage(int barcode)
        {
            var user = await GetUserAsync();

            var httpRequest = HttpContext.Request;

            return Json("");
        }

        [HttpGet]
        [BasicAuthorization]
        public async Task<JsonResult> Orders(string orderstatus = "", string searchuser = "", string paymentstatus = "", int sortbyorder = 1, int sortbyid = 0,
                        string search = "", int pageFrom = 1, int pageTotal = 10,
                        string datefrom = "", string dateto = "")
        {
            var user = await GetUserAsync();

            if (user.UserRole != null && user.UserRole == "ADMIN")
            {
                DateTime DateFrom = DateTime.MinValue;
                DateTime DateTo = DateTime.MaxValue;
                try
                {
                    DateFrom = datefrom == null || datefrom == "" ? DateTime.MinValue : FormatDate(datefrom, true);
                    DateTo = dateto == null || dateto == "" ? DateTime.MaxValue : FormatDate(dateto, false);
                }
                catch (Exception ex)
                {
                }
                orderstatus = orderstatus == null ? "" : (orderstatus == ("select status").ToLower() ? "" : orderstatus);
                searchuser = searchuser == null ? "" : (searchuser == ("select user").ToLower() ? "" : searchuser);
                paymentstatus = paymentstatus == null ? "" : (paymentstatus == ("select payment status").ToLower() ? "" : paymentstatus.ToLower());
                search = search == null ? "" : search;

                var _adminUserOrderViewModel = _adminServices.GetAdminUserOrderViewModel(user, orderstatus, paymentstatus, searchuser, sortbyorder,
                    sortbyid, search, pageFrom, pageTotal, DateFrom, DateTo);

                return Json(_adminUserOrderViewModel);
            }
            else
            {
                return Json("");
            }
        }

        [HttpGet]
        [BasicAuthorization]
        public async Task<JsonResult> OrderDetails(string order = "")
        {
            var user = await GetUserAsync();

            if (user.UserRole != null && user.UserRole == "ADMIN")
            {
                var model = _adminServices.GetOrderDetails(order);

                return Json(model);
            }
            else
            {
                return Json("");
            }
        }
    }
}
