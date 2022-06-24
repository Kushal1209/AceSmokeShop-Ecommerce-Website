using AceSmokeShop.Areas.Identity.Data;
using AceSmokeShop.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using AceSmokeShop.ViewModel;
using AceSmokeShop.Core.Repositories;
using AceSmokeShop.Models;
using System.Linq;
using AceSmokeShop.Services;

namespace AceSmokeShop.Controllers
{

    public class UserAccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly DBContext _context;
        private readonly ILogger<UserAccountController> _logger;
        public AdminUserViewModel _adminUserViewModel;
        public PaymentServices _paymentServices;
        private readonly AdminServices _adminServices;

        public UserAccountController(ILogger<UserAccountController> logger,
            UserManager<AppUser> userManager,
            DBContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
            _adminUserViewModel = new AdminUserViewModel();
            _paymentServices = new PaymentServices(new ProductRepository(context, logger),
                new CategoryRepository(context, logger), new SubCategoryRepository(context, logger),
                new StateRepository(context, logger), userManager, new CartRepository(context, logger), 
                new AddressRepository(context, logger), new TransactionRepository(context, logger));
            _adminServices = new AdminServices(new ProductRepository(context, logger),
                new CategoryRepository(context, logger), new SubCategoryRepository(context, logger),
                new StateRepository(context, logger), userManager, new CartRepository(context, logger), new AddressRepository(context, logger), _paymentServices,
                new UserOrdersRepository(context, logger), new OrderItemRepository(context, logger), new TransactionRepository(context, logger));
        }

        [HttpGet]
        public async Task<IActionResult> UserAccount(int StateID = 0, string search = "", string UserRole = "", int pageFrom = 1, int pageTotal = 5)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user.UserRole != null && user.UserRole == "ADMIN")
            {

                _adminUserViewModel = await _adminServices.GetAdminUserAccountAsync(user, StateID, search, UserRole, pageFrom, pageTotal);
                _adminUserViewModel.StateList.Insert(0, new State { StateID = 0, StateName = "Select State" });
                _adminUserViewModel.StateID = StateID;
                _adminUserViewModel.UserRole = UserRole;
                _adminUserViewModel.Search = search;
                _adminUserViewModel.CurrentPage = pageFrom;
                _adminUserViewModel.ItemsPerPage = pageTotal;
                _adminUserViewModel.StateSelect = _adminUserViewModel.StateList.Where(x => x.StateID == StateID).FirstOrDefault();

                return View("UserAccount", _adminUserViewModel);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUserAccount(string Email)
        {
            var user = await _userManager.GetUserAsync(User);
            EditUserViewModel editUserViewModel = new EditUserViewModel();
            editUserViewModel.UserEmail = Email;
            var CurrUser = _userManager.Users.Where(x => x.Email == Email).FirstOrDefault();
            editUserViewModel.UserRole = CurrUser.UserRole;
            editUserViewModel.IsActive = CurrUser.LockoutEnabled;
            editUserViewModel.IsAccounting = CurrUser.IsAccounting;
            return View("EditUserAccount", editUserViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserAccount(EditUserViewModel editUserViewModel)
        {
            var user = await _userManager.GetUserAsync(User);

            string result = await _adminServices.editUserAsync(user, editUserViewModel);
            if (result.ToLower().Equals("success"))
            {
                return View(editUserViewModel);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

    }
}
