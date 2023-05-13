using HomemadeLMS.Environment;
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

        public AccountController(IStorage<string, Account> accountStorage) : base(accountStorage)
        {
            SectionRootPath = AccountRootPath;
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
            if (newPassword is null || !Account.HasPasswordValidFormat(newPassword))
            {
                return View("Status", ActionStatus.PasswordInvalidFormat);
            }
            if (newPassword != passwordConfirmation)
            {
                return View("Status", ActionStatus.PasswordConfirmationError);
            }
            account.SetPassword(newPassword);
            await accountStorage.Update(account);
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
            if (!Program.SecretManager.TryGet(SecretName.ManagerToken, out string managerToken))
            {
                return View("Status", ActionStatus.HasNoManagerToken);
            }
            if (managerToken != token)
            {
                return View("Status", ActionStatus.InvalidManagerToken);
            }
            account.Role = UserRole.Manager;
            await accountStorage.Update(account);
            return RedirectPermanent(AccountRootPath);
        }

        [HttpGet]
        [RequireHttps]
        [Route(SignInPath)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult SignIn_Get()
        {
            return View("SignInByPass");
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
            var username = Account.GetUsername(parser.GetString("accountId"));
            var password = parser.GetString("password");
            if (username is null || !Account.HasUsernameValidFormat(username))
            {
                return View("Status", ActionStatus.UserNotFound);
            }
            var account = await accountStorage.Find(username);
            if (account is null)
            {
                return View("Status", ActionStatus.UserNotFound);
            }
            if (!account.IsPasswordCorrect(password))
            {
                return View("Status", ActionStatus.PasswordIsNotCorrect);
            }
            return await SignIn_Result(username);
        }

        [HttpGet]
        [RequireHttps]
        [Route(SignInPath + "/mail")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult SignIn_ByMail_Get()
        {
            return View("SignInByMail");
        }

        [HttpPost]
        [RequireHttps]
        [Route(SignInPath + "/mail")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> SignIn_ByMail_Post()
        {
            if (!Request.HasFormContentType)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var parser = new FormParser(Request.Form);
            var username = Account.GetUsername(parser.GetString("accountId"));
            if (username is null || !Account.HasUsernameValidFormat(username))
            {
                return View("Status", ActionStatus.UsernameInvalidFormat);
            }
            var emailAdress = Account.GetEmailAddress(username);
            await Program.AuthService.CreateRequest(emailAdress);
            return View("MailSent", emailAdress);
        }

        [HttpGet]
        [RequireHttps]
        [Route(SignInPath + "/confirm")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> SignIn_Confirm_Get(string? token)
        {
            var emailAdress = Program.AuthService.GetEmailAddress(token);
            var username = Account.GetUsername(emailAdress);
            if (username is null)
            {
                return View("Status", ActionStatus.InvalidConfirmationUrl);
            }
            var account = await accountStorage.Find(username);
            if (account is null)
            {
                account = new Account(username, UserRole.Student);
                if (!await accountStorage.TryInsert(account))
                {
                    return View("Status", ActionStatus.UnknownError);
                }
            }
            return await SignIn_Result(account.Username);
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

        private async Task<IActionResult> SignIn_Result(string username)
        {
            var claims = new List<Claim> { new Claim(ClaimsIdentity.DefaultNameClaimType, username) };
            ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
            return RedirectPermanent(AccountRootPath);
        }
    }
}