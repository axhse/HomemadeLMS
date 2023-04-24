﻿using HomemadeLMS.Models.Domain;
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
            var requestMaker = await GetAccount();
            if (requestMaker is null)
            {
                return RedirectPermanent(SignInPath);
            }
            Account? targetAccount;
            username ??= string.Empty;
            username = username.Trim().ToLower();
            if (username == string.Empty || username == requestMaker.Username)
            {
                targetAccount = requestMaker;
            }
            else
            {
                username = Account.GetUsername(username);
                if (!Account.HasUsernameValidFormat(username))
                {
                    return View("Status", ActionStatus.NotFound);
                }
                targetAccount = await accountStorage.Find(username);
                if (targetAccount is null)
                {
                    return View("Status", ActionStatus.NotFound);
                }
            }
            return View("Account", new AccountAndObject<Account>(requestMaker, targetAccount));
        }

        [HttpPost]
        [RequireHttps]
        [Route(AccountRootPath)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Account_Post(string? username)
        {
            if (username is null || !Account.HasUsernameValidFormat(username)
                || !Request.HasFormContentType)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var requestMaker = await GetAccount();
            if (requestMaker is null)
            {
                return RedirectPermanent(SignInPath);
            }
            Account? targetAccount = await accountStorage.Find(username);
            if (targetAccount is null || !requestMaker.CanChangeRoleOf(targetAccount))
            {
                return View("Status", ActionStatus.NotSupported);
            }
            if (new FormParser(Request.Form).TryGetUserRole("roleCode", out UserRole role))
            {
                targetAccount.Role = role;
                targetAccount.HeadUsername = role == UserRole.Manager ? requestMaker.Username : null;
                await accountStorage.Update(targetAccount);
            }
            return View("Account", new AccountAndObject<Account>(requestMaker, targetAccount));
        }

        [HttpGet]
        [RequireHttps]
        [Route(SignInPath)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Signin_Get()
        {
            return View("SignIn");
        }

        [HttpPost]
        [RequireHttps]
        [Route(SignInPath)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Signin_Post()
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

            if (!await accountStorage.HasKey(username))
            {
                var newAccount = new Account(username, "password123")
                {
                    Role = role
                };
                if (!await accountStorage.TryInsert(newAccount))
                {
                    return View("Status", ActionStatus.UnknownError);
                }
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