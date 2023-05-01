using HomemadeLMS.Models;
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
        public const string AccountRootPath = "/account";
        private IStorage<string, Account> accountStorage;

        public AccountController(IStorage<string, Account> accountStorage) : base(accountStorage)
        {
            SectionRootPath = AccountRootPath;
            this.accountStorage = accountStorage;
        }

        [HttpGet]
        [RequireHttps]
        [Route(AccountRootPath)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Account_Get(string? username)
        {
            username = Account.GetUsername(username);
            username ??= string.Empty;
            if (username != string.Empty && !Account.HasUsernameValidFormat(username))
            {
                return View("Status", ActionStatus.NotFound);
            }
            var requestMaker = await GetAccount();
            if (requestMaker is null)
            {
                return RedirectPermanent(SignInPath);
            }
            Account? targetAccount;
            if (username == string.Empty || username == requestMaker.Username)
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
            return View("Account", new AccountAndObject<Account>(requestMaker, targetAccount));
        }

        [HttpGet]
        [RequireHttps]
        [Route(AccountRootPath + "/changepassword")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Account_ChangePassword_Get()
        {
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            return View("ChangePassword");
        }

        [HttpPost]
        [RequireHttps]
        [Route(AccountRootPath + "/changepassword")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Account_ChangePassword_Post()
        {
            if (!Request.HasFormContentType)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var parser = new FormParser(Request.Form);
            var newPassword = parser.GetString("password");
            var passwordConfirmation = parser.GetString("confirmation");
            if (!Account.HasPasswordValidFormat(newPassword))
            {
                return View("Status", ActionStatus.PasswordInvalidFormat);
            }
            if (newPassword != passwordConfirmation)
            {
                return View("Status", ActionStatus.PasswordConfirmationError);
            }
            return RedirectPermanent(AccountRootPath);
        }

        [HttpGet]
        [RequireHttps]
        [Route(AccountRootPath + "/edit")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Account_Edit_Get(string? username)
        {
            username = Account.GetUsername(username);
            username ??= string.Empty;
            if (username != string.Empty && !Account.HasUsernameValidFormat(username))
            {
                return View("Status", ActionStatus.NotFound);
            }
            var requestMaker = await GetAccount();
            if (requestMaker is null)
            {
                return RedirectPermanent(SignInPath);
            }
            Account? targetAccount;
            if (username == string.Empty || username == requestMaker.Username)
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
            if (requestMaker.Username != targetAccount.Username
                && !requestMaker.CanChangeRoleOf(targetAccount))
            {
                return View("Status", ActionStatus.NoPermission);
            }
            return View("EditAccount", new AccountAndObject<Account>(requestMaker, targetAccount));
        }

        [HttpPost]
        [RequireHttps]
        [Route(AccountRootPath + "/edit")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Account_Edit_Post(string? username)
        {
            if (!Request.HasFormContentType)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            username = Account.GetUsername(username);
            username ??= string.Empty;
            if (username != string.Empty && !Account.HasUsernameValidFormat(username))
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var requestMaker = await GetAccount();
            if (requestMaker is null)
            {
                return RedirectPermanent(SignInPath);
            }
            Account? targetAccount;
            if (username == string.Empty || username == requestMaker.Username)
            {
                targetAccount = requestMaker;
            }
            else
            {
                targetAccount = await accountStorage.Find(username);
                if (targetAccount is null)
                {
                    return View("Status", ActionStatus.NotSupported);
                }
            }
            if (requestMaker.Username != targetAccount.Username
                && !requestMaker.CanChangeRoleOf(targetAccount))
            {
                return View("Status", ActionStatus.NoPermission);
            }

            var parser = new FormParser(Request.Form);
            if (requestMaker == targetAccount)
            {
                var name = DataUtils.CleanSpaces(parser.GetString("name"));
                var telegramUsername = DataUtils.GetTrimmed(parser.GetString("telegramUsername"));
                if (Account.HasNameValidFormat(name))
                {
                    targetAccount.Name = name;
                }
                if (Account.HasTelegramUsernameValidFormat(telegramUsername))
                {
                    targetAccount.TelegramUsername = telegramUsername;
                }
            }
            else if (requestMaker.CanChangeRoleOf(targetAccount))
            {
                if (parser.TryGetUserRole("roleCode", out UserRole role))
                {
                    targetAccount.Role = role;
                    targetAccount.HeadUsername = role == UserRole.Manager
                                                 ? requestMaker.Username : null;
                }
            }
            await accountStorage.Update(targetAccount);
            return RedirectPermanent($"{AccountRootPath}?username={targetAccount.Username}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(AccountRootPath + "/promote")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Promote_Get(string? token)
        {
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var managerToken = Program.AppConfig.ServiceConfig.ManagerToken;
            if (managerToken is null)
            {
                return View("Status", ActionStatus.HasNoToken);
            }
            if (managerToken != token)
            {
                return View("Status", ActionStatus.InvalidToken);
            }
            account.Role = UserRole.Manager;
            await accountStorage.Update(account);
            Program.AppConfig.ServiceConfig.DeleteManagerToken();
            return RedirectPermanent(AccountRootPath);
        }

        [HttpGet]
        [RequireHttps]
        [Route(SignInPath)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult SignIn_Get()
        {
            return View("SignIn");
        }

        [HttpPost]
        [RequireHttps]
        [Route(SignInPath)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> SignIn_Post()
        {
            if (!Request.HasFormContentType)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var parser = new FormParser(Request.Form);
            if (!parser.TryGetUserRole("roleCode", out UserRole role)
                || !parser.TryGetInt("id", out int id))
            {
                return View("Status", ActionStatus.InvalidFormData);
            }
            var username = role.ToString().ToLower() + id.ToString();

            if (await accountStorage.Find(username) is null)
            {
                var newAccount = new Account(username, "password123") { Role = role };
                await accountStorage.TryInsert(newAccount);
            }
            var claims = new List<Claim> { new Claim(ClaimsIdentity.DefaultNameClaimType, username) };
            ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
            return RedirectPermanent(AccountRootPath);
        }

        [HttpGet]
        [RequireHttps]
        [Route(SignOutPath)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> SignOut_Get()
        {
            await HttpContext.SignOutAsync();
            return RedirectPermanent(SignInPath);
        }
    }
}