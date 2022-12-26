using Microsoft.AspNetCore.Mvc;

namespace HomemadeLMS.Controllers
{
    public class BaseController : Controller
    {
        public RedirectResult RedirectToHomepage()
        {
            return RedirectPermanent(GetHomepagePath());
        }

        protected virtual string GetHomepagePath() => "/";
    }
}