using Microsoft.AspNetCore.Mvc;

namespace HomemadeLMS.Controllers
{
    public class HomeController : BaseController
    {
        private const string SectionPath = "/home";

        [HttpGet]
        [RequireHttps]
        [Route(DefaultPath)]
        [Route(SectionPath)]
        public IActionResult Homepage_Get()
        {
            return View("Homepage");
        }
    }
}