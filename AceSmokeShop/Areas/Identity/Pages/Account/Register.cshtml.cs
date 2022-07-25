using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using AceSmokeShop.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using AceSmokeShop.Models;
using AceSmokeShop.Data;
using Stripe;
using AceSmokeShop.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Razor;
using AuthorizeNet;

namespace AceSmokeShop.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly MyEmailSender _emailSender;
        private readonly DBContext _context;

        public RegisterModel(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ILogger<RegisterModel> logger,
            DBContext context,
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            ITempDataProvider tempDataProvider,
            IRazorViewEngine razorViewEngine)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
            _emailSender = new MyEmailSender(configuration, serviceProvider, tempDataProvider, razorViewEngine);
            States = (from addre in _context.tbl_state
                      select addre).ToList();
            States.Insert(0, new State { StateID = 0, StateName = "Select State" });
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<State> States { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Display(Name = "First Name")]
            public string Fullname { get; set; }

            [Required]
            [Display(Name = "Contact No.")]
            public string Contact { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
            public DateTime Dob { get; set; }

            [Required]
            [Display(Name = "State")]
            public int StateID { get; set; }

            [Display(Name ="State")]
            public virtual State State { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {

            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new AppUser { Fullname = Input.Fullname, UserName = Input.Email, Contact = Input.Contact,
                    Dob = Input.Dob, Email = Input.Email, StateID = Input.State.StateID, UserRole = "USER", LockoutEnabled = false, CreateDate= DateTime.Now, Stores = "None"};
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Create a user in Stripe
                    var parameters = new CustomerListOptions
                    {
                        Email = Input.Email
                    };
                    var Listservice = new CustomerService();
                    StripeList<Customer> customers = Listservice.List(parameters);
                    if (customers.Count() == 0)
                    {
                        var metaData = new Dictionary<string, string>();
                        metaData.Add("UserRole", "User");
                        var options = new CustomerCreateOptions
                        {
                            Email = Input.Email,
                            Name = Input.Fullname,
                            Phone = Input.Contact,
                            Metadata = metaData
                        };
                        var service = new CustomerService();
                        var stripecustomer = service.Create(options);

                        var thisUser = await _userManager.FindByIdAsync(user.Id);
                        thisUser.CustomerId = stripecustomer.Id;
                        thisUser.LockoutEnabled = false;
                        await _userManager.UpdateAsync(thisUser);

                    }
                    else
                    {
                        var thisUser = await _userManager.FindByIdAsync(user.Id);
                        thisUser.CustomerId = customers.FirstOrDefault().Id;
                        thisUser.LockoutEnabled = false;
                        await _userManager.UpdateAsync(thisUser);
                    }


                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    //_emailSender.SendEmail(Input.Email, "Confirm your email",
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    var body = await _emailSender.GetHtmlBody("ConfirmYourAccount", callbackUrl);

                        _emailSender.SendEmail(Input.Email, "Confirm your email", body);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
