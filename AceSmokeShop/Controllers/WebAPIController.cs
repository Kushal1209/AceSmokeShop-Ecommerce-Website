using AceSmokeShop.Areas.Identity.Data;
using AceSmokeShop.Core.Repositories;
using AceSmokeShop.Data;
using AceSmokeShop.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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
                var model = _adminServices.GetAdminFinancialViewModel(user, pageFrom, pageTotal, period, DateFrom, DateTo);

                return Json(model);
            }
            else
            {
                return Json("");
            }
        }
    }
}
