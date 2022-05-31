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
                        string search = "", int currentpage = 1, int totalpages = 0, int itemsperpage = 10)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user.UserRole != null && user.UserRole == "ADMIN")
            {
                _adminUserOrderViewModel = _adminServices.GetAdminUserOrderViewModel(user, orderstatus, sortbyorder,
                    sortbyid, search, currentpage, totalpages, itemsperpage);

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

    }
}
