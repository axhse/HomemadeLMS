using HomemadeLMS.Models;
using HomemadeLMS.Models.Domain;
using HomemadeLMS.Services.Data;
using HomemadeLMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace HomemadeLMS.Controllers
{
    public class CourseController : ControllerWithAccounts
    {
        public const string CourseRootPath = "/course";
        private readonly IStorage<int, Course> courseStorage;
        private readonly IStorage<int, CourseMember> courseMemberStorage;
        private readonly CourseAggregator courseAggregator;

        public CourseController(IStorage<string, Account> accountStorage,
            IStorage<int, Course> courseStorage,
            IStorage<int, CourseMember> courseMemberStorage,
            CourseAggregator courseAggregator) : base(accountStorage)
        {
            SectionRootPath = CourseRootPath;
            this.courseStorage = courseStorage;
            this.courseMemberStorage = courseMemberStorage;
            this.courseAggregator = courseAggregator;
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Course_Get(int id)
        {
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            if (id <= 0)
            {
                List<Course> allCourses;
                if (account.Role == UserRole.Manager)
                {
                    allCourses = await courseStorage.Select(_ => true);
                }
                else
                {
                    allCourses = await courseAggregator.GetUserCourses(account.Username);
                    var ownedCourses = await courseStorage.Select(
                        course => course.OwnerUsername == account.Username
                    );
                    bool IsIdSelected(int id) => allCourses.Any(course => course.Id == id);
                    var notSelectedCourses = ownedCourses.Where(course => !IsIdSelected(course.Id));
                    allCourses = allCourses.Concat(notSelectedCourses).ToList();
                }
                var model = new AccountAndObject<List<Course>>(account, allCourses);
                return View("CourseList", model);
            }
            Course? course = await courseStorage.Find(id);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            if (!await CanViewCourse(account, course))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            return View("Course", new AccountAndObject<Course>(account, course));
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath)]
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
                id = course.Id;
                if (account.Role == UserRole.Teacher)
                {
                    var courseMember = new CourseMember(
                        course.Id, account.Username, CourseRole.Teacher
                    );
                    if (!await courseMemberStorage.TryInsert(courseMember))
                    {
                        return View("Status", ActionStatus.UnknownError);
                    }
                }
            }
            return RedirectPermanent($"{CourseRootPath}/edit?id={id}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/edit")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> EditCourse_Get(int id)
        {
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            if (id <= 0)
            {
                return View("Status", ActionStatus.NotFound);
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
        [Route(CourseRootPath + "/edit")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> EditCourse_Post(int id)
        {
            if (id <= 0 || !Request.HasFormContentType)
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
            var ownerUsername = Account.GetUsername(parser.GetString("ownerUsername"));
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
            if (Account.HasUsernameValidFormat(ownerUsername))
            {
                course.OwnerUsername = ownerUsername;
            }
            await courseStorage.Update(course);
            return RedirectPermanent($"{CourseRootPath}?id={course.Id}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/members")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> CourseMemebrs_Get(int courseId)
        {
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            if (courseId <= 0)
            {
                return View("Status", ActionStatus.NotFound);
            }
            Course? course = await courseStorage.Find(courseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            if (!await CanViewCourse(account, course))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            var members = await courseMemberStorage.Select(member => member.CourseId == courseId);
            return View("CourseMembers", new CourseMembersVM(account, course, members));
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/members")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult CourseMemebrs_Post(int courseId)
        {
            if (courseId <= 0 || !Request.HasFormContentType)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var parser = new FormParser(Request.Form);
            var actionCode = parser.GetString("actionCode");
            if (actionCode != "add" && actionCode != "remove")
            {
                return View("Status", ActionStatus.NotSupported);
            }
            return RedirectPermanent($"{CourseRootPath}/members/{actionCode}?courseId={courseId}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/members/add")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> AddMembers_Get(int courseId)
        {
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            if (courseId <= 0)
            {
                return View("Status", ActionStatus.NotFound);
            }
            Course? course = await courseStorage.Find(courseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            if (!course.CanBeEditedBy(account))
            {
                return View("Status", ActionStatus.NoPermission);
            }
            return View("AddMembers", course);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/members/add")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> AddMembers_Post(int courseId)
        {
            if (courseId <= 0 || !Request.HasFormContentType)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            Course? course = await courseStorage.Find(courseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            if (!course.CanBeEditedBy(account))
            {
                return View("Status", ActionStatus.NoPermission);
            }
            var parser = new FormParser(Request.Form);
            var accountIdText = parser.GetString("accountIdText");
            accountIdText ??= string.Empty;
            accountIdText = Regex.Replace(accountIdText, @"[\s,;]", " ");
            var accountIds = accountIdText.Split(" ", StringSplitOptions.RemoveEmptyEntries)
                                          .ToHashSet();
            var model = new MemberChangelogVM(course);
            foreach (var accountId in accountIds)
            {
                var username = Account.GetUsername(accountId);
                if (!Account.HasUsernameValidFormat(username))
                {
                    model.InvalidUsernames.Add(username);
                    continue;
                }
                var courseMember = new CourseMember(courseId, username, CourseRole.Student);
                if (await courseMemberStorage.TryInsert(courseMember))
                {
                    model.AddedUsernames.Add(username);
                }
                else
                {
                    model.AlreadyAddedUsernames.Add(username);
                }
            }
            return View("MemberChangelog", model);
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/members/remove")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> RemoveMembers_Get(int courseId)
        {
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            if (courseId <= 0)
            {
                return View("Status", ActionStatus.NotFound);
            }
            Course? course = await courseStorage.Find(courseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            if (!course.CanBeEditedBy(account))
            {
                return View("Status", ActionStatus.NoPermission);
            }
            return View("RemoveMembers", course);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/members/remove")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> RemoveMembers_Post(int courseId)
        {
            if (courseId <= 0 || !Request.HasFormContentType)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            Course? course = await courseStorage.Find(courseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            if (!course.CanBeEditedBy(account))
            {
                return View("Status", ActionStatus.NoPermission);
            }
            var parser = new FormParser(Request.Form);
            var accountIdText = parser.GetString("accountIdText");
            accountIdText ??= string.Empty;
            accountIdText = accountIdText.Replace("\n", " ").Replace("\t", " ")
                                   .Replace(",", " ").Replace(";", " ");
            var accountIds = accountIdText.Split(" ", StringSplitOptions.RemoveEmptyEntries)
                                          .ToHashSet();

            var model = new MemberChangelogVM(course);
            foreach (var accountId in accountIds)
            {
                var username = Account.GetUsername(accountId);
                if (!Account.HasUsernameValidFormat(username))
                {
                    model.InvalidUsernames.Add(username);
                    continue;
                }
                var courseMembers = await courseMemberStorage.Select(
                    member => member.CourseId == courseId && member.Username == username
                );
                if (!courseMembers.Any())
                {
                    model.AlreadyRemovedUsernames.Add(username);
                    continue;
                }
                foreach(var courseMember in courseMembers)
                {
                    await courseMemberStorage.TryDeleteValue(courseMember);
                }
                model.RemovedUsernames.Add(username);
            }
            return View("MemberChangelog", model);
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/member")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> CourseMember_Get(int courseId, string? username)
        {
            if (username is null || !Account.HasUsernameValidFormat(username) || courseId <= 0)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var course = await courseStorage.Find(courseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var courseMember = await GetCourseMember(courseId, username);
            if (courseMember is null || !course.CanBeEditedBy(account))
            {
                return View("Status", ActionStatus.NotFound);
            }
            return View("CourseMember", new CourseMemberVM(account, course, courseMember));
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/member")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> CourseMember_Post(int courseId, string? username)
        {
            if (username is null || !Account.HasUsernameValidFormat(username)
                || courseId <= 0 || !Request.HasFormContentType)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var course = await courseStorage.Find(courseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var courseMember = await GetCourseMember(courseId, username);
            if (courseMember is null || !course.CanBeEditedBy(account))
            {
                return View("Status", ActionStatus.NotSupported);
            }
            if (new FormParser(Request.Form).TryGetCourseRole("roleCode", out CourseRole role))
            {
                courseMember.Role = role;
                await courseMemberStorage.Update(courseMember);
            }
            return View("CourseMember", new CourseMemberVM(account, course, courseMember));
        }

        private async Task<bool> CanViewCourse(Account account, Course course)
        {
            if (account.Role == UserRole.Manager || account.Username == course.OwnerUsername)
            {
                return true;
            }
            return await GetCourseMember(course.Id, account.Username) is not null;
        }

        private async Task<CourseMember?> GetCourseMember(int courseId, string username)
        {
            var members = await courseMemberStorage.Select(
                    member => member.CourseId == courseId && member.Username == username
            );
            return members.FirstOrDefault();
        }
    }
}