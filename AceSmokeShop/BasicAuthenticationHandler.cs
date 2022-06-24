using AceSmokeShop.Areas.Identity.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AceSmokeShop
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock, 
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager) : base(options, logger, encoder, clock)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return await Task.FromResult(AuthenticateResult.Fail("Authorization Header Missing"));
            }

            var authorizationheader = Request.Headers["Authorization"].ToString();
            var authHeaderRegex = new Regex("Basic (.*)");
            if (!authHeaderRegex.IsMatch(authorizationheader))
            {
                return await Task.FromResult(AuthenticateResult.Fail("Authorization Code not formatted Correctly"));
            }

            var authBase64 = Encoding.UTF8.GetString(Convert.FromBase64String(authHeaderRegex.Replace(authorizationheader, "$1")));

            var creds = authBase64.Split(':', 2);
            var authUsername = creds[0];
            var authPassword = creds.Length > 1 ? creds[1] : throw new Exception("Unable to get Password");

            //Authenticate User
            var user = await InternalAuthenticationAsync(authUsername, authPassword);

            if(user.GetType() == typeof(string))
            {
                return await Task.FromResult(AuthenticateResult.Fail(user.ToString()));
            }
            AppUser AuthUser = (AppUser)user;
            var authenticatedUser = new AuthenticatedUser("BasicAuthentication", true, AuthUser.UserName);
            var cliamsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(authenticatedUser));

            return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(cliamsPrincipal, Scheme.Name)));
        }

        private async Task<Object> InternalAuthenticationAsync(string authUsername, string authPassword)
        {
            var user = await _userManager.FindByEmailAsync(authUsername);

            if (user == null)
            {
                return "Invalid Login Attempt";
            }
            if (user.LockoutEnabled == true)
            {
                return "UserLockedOut";
            }
            if (user.IsAccounting == false)
            {
                return "UnAuthorized Login Attempt";
            }

            var result = await _signInManager.PasswordSignInAsync(authUsername, authPassword, false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return user;
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
