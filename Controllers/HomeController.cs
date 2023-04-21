using HomemadeLMS.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HomemadeLMS.Controllers
{
    public class HomeController : BaseController
    {
        public const string ErrorPath = "/error";
        private const string SectionPath = "/home";

        [HttpGet]
        [RequireHttps]
        [Route(DefaultPath)]
        [Route(SectionPath)]
        public IActionResult Homepage_Get()
        {
            return View("Homepage");
        }

        [HttpGet]
        [RequireHttps]
        [Route(ErrorPath)]
        public IActionResult Error_Get()
        {
            return View("Status", ActionStatus.InternalError);
        }
    }
}