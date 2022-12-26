using HomemadeLMS.Models.Domain;
using HomemadeLMS.Services.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomemadeLMS.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IStorage<string, Account> accountStorageService;

        public HomeController(IStorage<string, Account> accountStorageService)
        {
            this.accountStorageService = accountStorageService;
        }

        [Route("/")]
        [Route("/index")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("/create")]
        public IActionResult CreateAccount(string username)
        {
            accountStorageService.TryInsert(new Account { Username = username });
            return Content("created");
        }

        [RequireHttps]
        [Route("/signin")]
        public IActionResult SignIn(string username)
        {
            Account? account = accountStorageService.Find(username);
            if (account is null)
            {
                return Content("account not found");
            }
            var claims = new List<Claim> { new Claim(ClaimsIdentity.DefaultNameClaimType, username) };
            ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.SignInAsync(new ClaimsPrincipal(identity)).Wait();
            return Content("signed in");
        }

        [RequireHttps]
        [Route("/signout")]
        public new IActionResult SignOut()
        {
            HttpContext.SignOutAsync().Wait();
            return Content("signed out");
        }

        [Route("/claims")]
        public IActionResult Claims()
        {
            var claims = HttpContext.User.Claims;
            return Content(string.Join(" | ", claims) + " .");
        }

        [Route("/account")]
        public IActionResult GetAccount(string username)
        {
            var account = accountStorageService.Find(username);
            if (account is null)
            {
                return Content("null");
            }
            return Content(account.Username);
        }
    }
}