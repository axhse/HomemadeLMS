using HomemadeLMS.Models.Domain;
using HomemadeLMS.Services.Data;
using HomemadeLMS.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomemadeLMS.Controllers
{
    public class AccountController : ControllerWithAccounts
    {
        private const string SectionPath = "/account";
        private const string SignInPath = SectionPath + "/signin";

        public AccountController(IStorage<string, Account> accountStorage) : base(accountStorage)
        { }

        [HttpGet]
        [RequireHttps]
        [Route(SectionPath)]
        public async Task<IActionResult> Account()
        {
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            return View(account);
        }

        [HttpGet]
        [RequireHttps]
        [Route(SignInPath)]
        public IActionResult Signin_Get()
        {
            return View("SignIn");
        }

        [HttpPost]
        [RequireHttps]
        [Route(SignInPath)]
        public async Task<IActionResult> Signin_Post()
        {
            var parser = new FormParser(Request.Form);
            if (!parser.TryGetInt("id", out int id))
            {
                return Content("invalid id");
            }
            if (!parser.TryGetUserRole("roleCode", out UserRole role))
            {
                return Content("invalid roleCode");
            }
            var username = role.ToString() + id.ToString();

            Account? account = accountStorage.Find(username);
            if (account is null)
            {
                var newAccount = new Account(username, "password123");
                accountStorage.TryInsert(newAccount);
            }
            var claims = new List<Claim> { new Claim(ClaimsIdentity.DefaultNameClaimType, username) };
            ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
            return RedirectToHomepage();
        }

        [HttpGet]
        [RequireHttps]
        [Route(SectionPath + "/signout")]
        public async Task<IActionResult> SignOut_Get()
        {
            await HttpContext.SignOutAsync();
            return RedirectPermanent(SignInPath);
        }

        protected override string GetHomepagePath() => SectionPath;
    }
}