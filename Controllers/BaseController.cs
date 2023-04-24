using HomemadeLMS.Models.Domain;
using HomemadeLMS.Services.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace HomemadeLMS.Controllers
{
    public class BaseController : Controller
    {
        public const string GlobalRootPath = "/";
        protected string SectionRootPath = GlobalRootPath;

        public IActionResult RedirectToHomepage()
        {
            return RedirectPermanent(SectionRootPath);
        }
    }

    public class ControllerWithAccounts : BaseController
    {
        public const string SignInPath = "/signin";
        public const string SignOutPath = "/signout";

        protected readonly IStorage<string, Account> accountStorage;
        private Account? currentAccount;

        public ControllerWithAccounts(IStorage<string, Account> accountStorage)
        {
            this.accountStorage = accountStorage;
        }

        protected async Task<Account?> GetAccount()
        {
            if (currentAccount is not null)
            {
                return currentAccount;
            }
            var claims = HttpContext.User.Claims;
            if (!claims.Any())
            {
                return null;
            }
            var username = claims.First().Value;
            Account? account = await accountStorage.Find(username);
            if (account is null)
            {
                await HttpContext.SignOutAsync();
                return null;
            }
            currentAccount = account;
            return account;
        }
    }
}