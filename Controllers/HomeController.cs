using HomemadeLMS.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HomemadeLMS.Controllers
{
    public class HomeController : BaseController
    {
        public const string ErrorPath = "/error";
        public const string HomeRootPath = "/home";

        [HttpGet]
        [RequireHttps]
        [Route(ErrorPath)]
        public IActionResult Error_Get()
        {
            return View("Status", ActionStatus.InternalError);
        }

        [HttpGet]
        [RequireHttps]
        [Route(GlobalRootPath)]
        [Route(HomeRootPath)]
        public IActionResult Homepage_Get()
        {
            return View("Homepage");
        }
    }
}