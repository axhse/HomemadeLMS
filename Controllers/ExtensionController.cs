using HomemadeLMS.Models.Domain;
using HomemadeLMS.Services.Data;
using HomemadeLMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static System.Formats.Asn1.AsnWriter;

namespace HomemadeLMS.Controllers
{
    public class ExtensionController : ControllerWithAccounts
    {
        private const string SectionPath = "/extension";
        private readonly IStorage<string, RoleTestResult> roleTestResultStorage;

        public ExtensionController(IStorage<string, Account> accountStorage,
            IStorage<string, RoleTestResult> roleTestResultStorage) : base(accountStorage)
        {
            this.roleTestResultStorage = roleTestResultStorage;
        }

        [HttpGet]
        [RequireHttps]
        [Route(SectionPath)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Extension_Get()
        {
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            return View("ExtensionMenu");
        }

        [HttpGet]
        [RequireHttps]
        [Route(SectionPath + "/roletest" + "/status")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> RoleTestStatus_Get()
        {
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var testResults = await roleTestResultStorage.Select(
                result => result.Username == account.Username
            );
            return View("RoleTestStatus", testResults.FirstOrDefault());
        }

        [HttpPost]
        [RequireHttps]
        [Route(SectionPath + "/roletest" + "/status")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult RoleTestStatus_Post()
        {
            return RedirectPermanent(SectionPath + "/roletest");
        }

        [HttpGet]
        [RequireHttps]
        [Route(SectionPath + "/roletest")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> RoleTest_Get()
        {
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            return View("RoleTest");
        }

        [HttpPost]
        [RequireHttps]
        [Route(SectionPath + "/roletest")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> RoleTest_Post()
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
            var testResult = new RoleTestResult(account.Username);
            var parser = new FormParser(Request.Form);
            for (int blockIndex = 1; blockIndex <= 7; blockIndex++)
            {
                var totalScore = 0;
                for (int ansIndex = 1; ansIndex <= 8; ansIndex++)
                {
                    if (!parser.TryGetInt($"b{blockIndex}a{ansIndex}Score", out var score)
                        || !RoleTestResult.IsScoreValid(score))
                    {
                        return View("Status", ActionStatus.InvalidFormData);
                    }
                    testResult.AddScore(blockIndex, ansIndex, score);
                    totalScore += score;
                }
                if (!RoleTestResult.IsBlockTotalScoreValid(totalScore))
                {
                    return View("Status", ActionStatus.InvalidFormData);
                }
            }
            if (!await roleTestResultStorage.TryInsert(testResult))
            {
                await roleTestResultStorage.Update(testResult);
            }
            return RedirectPermanent(SectionPath + "/roletest" + "/status");
        }

        protected override string GetHomepagePath() => SectionPath;
    }
}