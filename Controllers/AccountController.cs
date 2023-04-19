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
        public async Task<IActionResult> Account_Get(string? username)
        {
            var requestMaker = await GetAccount();
            if (requestMaker is null)
            {
                return RedirectPermanent(SignInPath);
            }
            Account? targetAccount;
            if (username is null || username == requestMaker.Username)
            {
                targetAccount = requestMaker;
            }
            else
            {
                targetAccount = await accountStorage.Find(username);
                if (targetAccount is null)
                {
                    return View("Status", ActionStatus.NotFound);
                }
            }
            return View("Account", new AccountVM(requestMaker, targetAccount));
        }

        [HttpPost]
        [RequireHttps]
        [Route(SectionPath)]
        public async Task<IActionResult> Account_Post(string? username)
        {
            if (username is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var requestMaker = await GetAccount();
            if (requestMaker is null)
            {
                return RedirectPermanent(SignInPath);
            }
            Account? targetAccount = await accountStorage.Find(username);
            if (targetAccount is null || !requestMaker.CanChangeRole(targetAccount))
            {
                return View("Status", ActionStatus.NotSupported);
            }
            if (new FormParser(Request.Form).TryGetUserRole("roleCode", out UserRole role))
            {
                targetAccount.Role = role;
                targetAccount.HeadUsername = role == UserRole.Manager ? requestMaker.Username : null;
                await accountStorage.Update(targetAccount);
            }
            return View("Account", new AccountVM(requestMaker, targetAccount));
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

            if (!await accountStorage.HasKey(username))
            {
                var newAccount = new Account(username, "password123")
                {
                    Role = role
                };
                await accountStorage.TryInsert(newAccount);
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