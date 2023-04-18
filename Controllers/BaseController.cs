using Microsoft.AspNetCore.Mvc;

namespace HomemadeLMS.Controllers
{
    public class BaseController : Controller
    {
        protected const string DefaultPath = "/";

        public IActionResult RedirectToHomepage()
        {
            return RedirectPermanent(GetHomepagePath());
        }

        protected virtual string GetHomepagePath() => DefaultPath;
    }
}