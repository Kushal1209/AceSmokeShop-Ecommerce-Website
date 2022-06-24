using AceSmokeShop.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    }
}
