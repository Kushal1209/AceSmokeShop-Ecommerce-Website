using AceSmokeShop.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AceSmokeShop.Controllers
{
    public class WebAPIController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;


        public WebAPIController(SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public string SayHii()
        {
            return "Hiiiiiiiiiiiiiiiiiiiiiiiiiiii";
        }

        public async Task<string> LoginAsync(string Email, string Password)
        {
            var user = await _userManager.FindByEmailAsync(Email);

            if (user == null)
            {
                return "Invalid login attempt.";
            }
            if (user.LockoutEnabled == true)
            {
                return "LockOut User";
            }
            var result = await _signInManager.PasswordSignInAsync(Email, Password, false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return "Bingo";
            }
            if (result.IsLockedOut)
            {
                return "LockOut-User";
            }
            else
            {
                return "Invalid login attempt.";
            }
        }
    }
}
