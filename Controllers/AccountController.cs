using HomemadeLMS.Models.Domain;
using HomemadeLMS.Services.Data;
using HomemadeLMS.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomemadeLMS.Controllers
{
    public class AccountController : BaseController
    {
        private const string SectionPath = "/account";
        private readonly IStorage<string, Account> accountStorage;

        public AccountController(IStorage<string, Account> accountStorage)
        {
            this.accountStorage = accountStorage;
        }

        [HttpGet]
        [RequireHttps]
        [Route(SectionPath)]
        public IActionResult Homepage()
        {
            var claims = HttpContext.User.Claims;
            return Content($"Claims[{claims.Count()}]{{{string.Join(", ", claims)}}}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(SectionPath + "/signin")]
        public IActionResult Signin_Get()
        {
            return View("SignIn");
        }

        [HttpPost]
        [RequireHttps]
        [Route(SectionPath + "/signin")]
        public IActionResult Signin_Post()
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

            var resultStatus = string.Empty;
            Account? account = accountStorage.Find(username);
            if (account is null)
            {
                var newAccount = new Account(username, "password123");
                accountStorage.TryInsert(newAccount);
                resultStatus = "created and ";
            }
            var claims = new List<Claim> { new Claim(ClaimsIdentity.DefaultNameClaimType, username) };
            ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.SignInAsync(new ClaimsPrincipal(identity)).Wait();
            resultStatus += $"signed in - {username}";
            return Content(resultStatus);
        }

        [HttpGet]
        [RequireHttps]
        [Route(SectionPath + "/signout")]
        public async Task<IActionResult> SignOut_Get()
        {
            await HttpContext.SignOutAsync();
            return RedirectPermanent(DefaultPath);
        }

        protected override string GetHomepagePath() => SectionPath;
    }
}