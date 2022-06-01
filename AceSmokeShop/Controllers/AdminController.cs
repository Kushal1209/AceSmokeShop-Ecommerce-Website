using AceSmokeShop.Areas.Identity.Data;
using AceSmokeShop.Areas.Identity.Pages.Account;
using AceSmokeShop.Core.Repositories;
using AceSmokeShop.Data;
using AceSmokeShop.Models;
using AceSmokeShop.Repository.IRepositories;
using AceSmokeShop.Services;
using AceSmokeShop.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AceSmokeShop.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly DBContext _context;
        private AdminUserOrderViewModel _adminUserOrderViewModel;
        public PaymentServices _paymentServices;
        private readonly AdminServices _adminServices;

        public AdminController(ILogger<ProductController> logger, UserManager<AppUser> userManager, 
            DBContext context)
        {
            _userManager = userManager;
            _context = context;
            _adminUserOrderViewModel = new AdminUserOrderViewModel();
            _paymentServices = new PaymentServices(new ProductRepository(context, logger),
                new CategoryRepository(context, logger), new SubCategoryRepository(context, logger),
                new StateRepository(context, logger), userManager, new CartRepository(context, logger), new AddressRepository(context, logger));
            _adminServices = new AdminServices(new ProductRepository(context, logger),
                new CategoryRepository(context, logger), new SubCategoryRepository(context, logger),
                new StateRepository(context, logger), userManager, new CartRepository(context, logger), new AddressRepository(context, logger), _paymentServices,
                new UserOrdersRepository(context, logger), new OrderItemRepository(context, logger));
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if(user.UserRole != null && user.UserRole == "ADMIN")
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index","Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Orders(string orderstatus = "", int sortbyorder = 1, int sortbyid = 0, 
                        string search = "", int pageFrom = 1, int pageTotal = 10, 
                        string datefrom = "", string dateto = "")
        {
            var user = await _userManager.GetUserAsync(User);

            if (user.UserRole != null && user.UserRole == "ADMIN")
            {
                DateTime DateFrom = datefrom == null || datefrom == "" ? DateTime.MinValue : DateTime.Parse(datefrom); 
                DateTime DateTo = dateto == null || dateto == "" ? DateTime.MaxValue : DateTime.Parse(dateto);
                orderstatus = orderstatus.ToLower() == ("Select Status").ToLower() ? "" : orderstatus;
                search = search == null ? "" : search;

                _adminUserOrderViewModel = _adminServices.GetAdminUserOrderViewModel(user, orderstatus, sortbyorder,
                    sortbyid, search, pageFrom, pageTotal, DateFrom, DateTo);

                return View("Orders", _adminUserOrderViewModel);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(string order = "")
        {
            var user = await _userManager.GetUserAsync(User);

            if (user.UserRole != null && user.UserRole == "ADMIN")
            {
                var model = _adminServices.GetOrderDetails(order);

                return PartialView("_AdminUserOrderPartialView", model);
            }
            else
            {
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUserOrderStatusGet(string orderid)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user.UserRole != null && user.UserRole == "ADMIN")
            {

                var orderstatus = _adminServices.GetOrderStatus(user, orderid);
                if (orderstatus == "") return StatusCode(500, "Something Went Wrong");

                return View("EditUserOrderStatus", orderstatus + ":" + orderid);
            }
            else
            {
                return StatusCode(401, "UnAuthorized");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserOrderStatus(string orderid = "", string orderstatus = "")
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _adminServices.EditUserOrderStatus(user, orderid, orderstatus);

            if (result.ToLower().Equals("success"))
            {
                return StatusCode(200);
            }
            else
            {
                return StatusCode(500, result);
            }
        }
    }
}
