using HomemadeLMS.Models.Domain;
using HomemadeLMS.Services.Data;
using HomemadeLMS.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HomemadeLMS.Controllers
{
    public class CourseController : ControllerWithAccounts
    {
        private const string SectionPath = "/course";
        private readonly IStorage<int, Course> courseStorage;

        public CourseController(IStorage<string, Account> accountStorage,
            IStorage<int, Course> courseStorage) : base(accountStorage)
        {
            this.courseStorage = courseStorage;
        }

        [HttpGet]
        [RequireHttps]
        [Route(SectionPath)]
        public async Task<IActionResult> Course_Get(int id)
        {
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            if (id == 0)
            {
                // TODO: all courses
                return Content("TODO: all courses");
            }
            Course? course = await courseStorage.Find(id);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            // TODO: check if allowed
            return View("Course", new CourseVM(account, course));
        }

        [HttpGet]
        [RequireHttps]
        [Route(SectionPath + "/create")]
        public async Task<IActionResult> CreateCourse()
        {
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            if (account.Role != UserRole.Teacher && account.Role != UserRole.Manager)
            {
                return View("Status", ActionStatus.NoAccess);
            }
            var course = new Course(account.Username);
            if (!await courseStorage.TryInsert(course)) {
                return View("Status", ActionStatus.UnknownError);
            }
            return RedirectPermanent($"{SectionPath}/edit?id={course.Id}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(SectionPath + "/edit")]
        public async Task<IActionResult> EditCourse_Get(int id)
        {
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            Course? course = await courseStorage.Find(id);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            if (!course.CanBeEditedBy(account))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            return View("EditCourse", course);
        }

        [HttpPost]
        [RequireHttps]
        [Route(SectionPath + "/edit")]
        public async Task<IActionResult> EditCourse_Post(int id)
        {
            if (id == 0 || !Request.HasFormContentType)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            Course? course = await courseStorage.Find(id);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            if (!course.CanBeEditedBy(account))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            var parser = new FormParser(Request.Form);
            var title = parser.GetString("title");
            var description = parser.GetString("description");
            var smartLmsUrl = parser.GetString("smartLmsUrl");
            var pldUrl = parser.GetString("pldUrl");
            if (title is null || !Course.HasTitleValidFormat(title)
                || !Course.HasDescriptionValidFormat(description)
                || !Course.HasUrlValidFormat(smartLmsUrl)
                || !Course.HasUrlValidFormat(pldUrl))
            {
                return View("Status", ActionStatus.InvalidFormData);
            }
            course.Title = title;
            course.Description = description;
            course.SmartLmsUrl = smartLmsUrl;
            course.PldUrl = pldUrl;
            await courseStorage.Update(course);
            return RedirectPermanent($"{SectionPath}?id={course.Id}");
        }

        protected override string GetHomepagePath() => SectionPath;
    }
}