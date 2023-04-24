using HomemadeLMS.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HomemadeLMS.Controllers
{
    public class HomeController : BaseController
    {
        public const string HomeRootPath = "/home";
        public const string ErrorPath = "/error";

        [HttpGet]
        [RequireHttps]
        [Route(GlobalRootPath)]
        [Route(HomeRootPath)]
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