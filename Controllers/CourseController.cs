using HomemadeLMS.Models;
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
        private readonly IStorage<int, CourseMember> courseMemberStorage;
        private readonly CourseAggregator courseAggregator;

        public CourseController(IStorage<string, Account> accountStorage,
            IStorage<int, Course> courseStorage,
            IStorage<int, CourseMember> courseMemberStorage,
            CourseAggregator courseAggregator) : base(accountStorage)
        {
            this.courseStorage = courseStorage;
            this.courseMemberStorage = courseMemberStorage;
            this.courseAggregator = courseAggregator;
        }

        [HttpGet]
        [RequireHttps]
        [Route(SectionPath)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Course_Get(int id)
        {
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            if (id == 0)
            {
                var allCourseInfo = await courseAggregator.GetUserCourses(account.Username);
                var model = new AccountAndObject<IEnumerable<CourseInfo>>(account, allCourseInfo);
                return View("CourseList", model);
            }
            Course? course = await courseStorage.Find(id);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var members = await courseMemberStorage.Select(
                member => member.CourseId == course.Id && member.Username == account.Username
            );
            if (!members.Any())
            {
                return View("Status", ActionStatus.NoAccess);
            }
            return View("Course", new CourseVM(account, course));
        }

        [HttpPost]
        [RequireHttps]
        [Route(SectionPath)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Course_Post(int id)
        {
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            if (!account.CanEditCourses)
            {
                return View("Status", ActionStatus.NoPermission);
            }
            if (id == 0)
            {
                var course = new Course(account.Username);
                if (!await courseStorage.TryInsert(course))
                {
                    return View("Status", ActionStatus.UnknownError);
                }
                var courseRole = account.Role == UserRole.Teacher
                                    ? CourseRole.Teacher : CourseRole.Manager;
                var courseMember = new CourseMember(course.Id, account.Username, courseRole);
                if (!await courseMemberStorage.TryInsert(courseMember))
                {
                    return View("Status", ActionStatus.UnknownError);
                }
                id = course.Id;
            }
            return RedirectPermanent($"{SectionPath}/edit?id={id}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(SectionPath + "/edit")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
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
                return View("Status", ActionStatus.NoPermission);
            }
            return View("EditCourse", course);
        }

        [HttpPost]
        [RequireHttps]
        [Route(SectionPath + "/edit")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
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
                return View("Status", ActionStatus.NoPermission);
            }
            var parser = new FormParser(Request.Form);
            var title = DataUtils.GetTrimmed(parser.GetString("title"));
            var description = DataUtils.GetTrimmed(parser.GetString("description"));
            var smartLmsUrl = DataUtils.GetTrimmed(parser.GetString("smartLmsUrl"));
            var pldUrl = DataUtils.GetTrimmed(parser.GetString("pldUrl"));
            if (title is not null && Course.HasTitleValidFormat(title))
            {
                course.Title = title;
            }
            if (Course.HasDescriptionValidFormat(description))
            {
                course.Description = description;
            }
            if (Course.HasUrlValidFormat(smartLmsUrl))
            {
                course.SmartLmsUrl = smartLmsUrl;
            }
            if (Course.HasUrlValidFormat(pldUrl))
            {
                course.PldUrl = pldUrl;
            }
            await courseStorage.Update(course);
            return RedirectPermanent($"{SectionPath}?id={course.Id}");
        }

        protected override string GetHomepagePath() => SectionPath;
    }
}