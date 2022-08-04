using AceSmokeShop.Areas.Identity.Data;
using AceSmokeShop.Core.Repositories;
using AceSmokeShop.Data;
using AceSmokeShop.Services;
using AceSmokeShop.ViewModel;
using Microsoft.AspNetCore.Authorization;
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
                new StateRepository(context, logger), userManager, new CartRepository(context, logger), 
                new AddressRepository(context, logger), new TransactionRepository(context, logger),
                new UserOrdersRepository(context, logger));
            _adminServices = new AdminServices(new ProductRepository(context, logger),
                new CategoryRepository(context, logger), new SubCategoryRepository(context, logger),
                new StateRepository(context, logger), userManager, new CartRepository(context, logger), new AddressRepository(context, logger), _paymentServices,
                new UserOrdersRepository(context, logger), new OrderItemRepository(context, logger), new TransactionRepository(context, logger));
        }

        public async Task<IActionResult> Index(int pageFrom = 1, int pageTotal = 10, int period = 0,
                        string datefrom = "", string dateto = "")
        {
            var user = await _userManager.GetUserAsync(User);

            if(user.UserRole != null && user.UserRole == "ADMIN")
            {
                DateTime DateFrom = datefrom == null || datefrom == "" ? DateTime.MinValue : DateTime.Parse(datefrom);
                DateTime DateTo;
                if(dateto == null || dateto == ""){
                    DateTo = DateTime.MaxValue;
                }
                else
                {
                    DateTo = DateTime.Parse(dateto);
                    DateTo = new DateTime(DateTo.Year, DateTo.Month, DateTo.Day, 23, 59, 59);

                }
                var model = _adminServices.GetAdminFinancialViewModel(user, pageFrom, pageTotal, period, DateFrom, DateTo);


                return View(model);
            }
            else
            {
                return RedirectToAction("Index","Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Orders(string orderstatus = "", string searchuser = "", string paymentstatus = "", int sortbyorder = 1, int sortbyid = 0, 
                        string search = "", int pageFrom = 1, int pageTotal = 10, 
                        string datefrom = "", string dateto = "")
        {
            var user = await _userManager.GetUserAsync(User);

            if (user.UserRole != null && user.UserRole == "ADMIN")
            {
                DateTime DateFrom = datefrom == null || datefrom == "" ? DateTime.MinValue : DateTime.Parse(datefrom);
                DateTime DateTo;
                if (dateto == null || dateto == "")
                {
                    DateTo = DateTime.MaxValue;
                }
                else
                {
                    DateTo = DateTime.Parse(dateto);
                    DateTo = new DateTime(DateTo.Year, DateTo.Month, DateTo.Day, 23, 59, 59);

                }
                orderstatus = orderstatus.ToLower() == ("select status").ToLower() ? "" : orderstatus;
                searchuser = searchuser.ToLower() == ("select user").ToLower() ? "" : searchuser;
                paymentstatus = paymentstatus.ToLower() == ("select payment status").ToLower() ? "" : paymentstatus.ToLower();
                search = search == null ? "" : search;

                _adminUserOrderViewModel = _adminServices.GetAdminUserOrderViewModel(user, orderstatus, paymentstatus, searchuser, sortbyorder,
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

        [HttpGet]
        public async Task<IActionResult> EditVendorOrderGet(string CustOrderId, int OrderId, int Qty)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user.UserRole != null && user.UserRole == "ADMIN")
            {

                var model = new EditVendorOrderViewModel();
                model.CustOrderId = CustOrderId;
                model.OrderId = OrderId;
                model.Qty = Qty;

                return View("EditVendorOrder", model);
            }
            else
            {
                return StatusCode(401, "UnAuthorized");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVendorOrder(EditVendorOrderViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _adminServices.EditVendorOrder(user, model.CustOrderId, model.OrderId, model.Qty);

            if (result.ToLower().Equals("success"))
            {
                return StatusCode(200);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteVendorOrderGet(int items, int OrderId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user.UserRole != null && user.UserRole == "ADMIN")
            {

                var model = new EditVendorOrderViewModel();

                model.items = items;
                model.OrderId = OrderId;

                return View("DeleteVendorOrder", model);
            }
            else
            {
                return StatusCode(401, "UnAuthorized");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVendorOrder(EditVendorOrderViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = _adminServices.DeleteVendorOrder(user, model.items, model.OrderId);

            if (result.ToLower().Equals("success"))
            {
                return StatusCode(200);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        public async Task<IActionResult> MarkAsPaidGet(string CustOrderId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user.UserRole != null && user.UserRole == "ADMIN")
            {
                var model = new EditVendorOrderViewModel();
                model.CustOrderId = CustOrderId;
                return View("MarkAsPaid", model);
            }
            else
            {
                return StatusCode(401, "UnAuthorized");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(string CustOrderId)
        {
            var user = await _userManager.GetUserAsync(User);
           
            if(user != null && user.UserRole != null && user.UserRole == "ADMIN")
            {
                var result = _adminServices.MarkAsPaid(user, CustOrderId);
                return StatusCode(200);
            }
            
            return StatusCode(401);
        }

    }
}
