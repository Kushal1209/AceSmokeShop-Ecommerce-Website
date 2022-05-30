using AceSmokeShop.Areas.Identity.Data;
using AceSmokeShop.Areas.Identity.Pages.Account;
using AceSmokeShop.Data;
using AceSmokeShop.Models;
using AceSmokeShop.Repository.IRepositories;
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

        public AdminController(UserManager<AppUser> userManager, 
            DBContext context)
        {
            _userManager = userManager;
            _context = context;
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
        
    }
}
