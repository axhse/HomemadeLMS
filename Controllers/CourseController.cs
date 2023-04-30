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
        private readonly IStorage<int, Announcement> announcementStorage;
        private readonly IStorage<int, Homework> homeworkStorage;
        private readonly IStorage<string, HomeworkStatus> homeworkStatusStorage;
        private readonly CourseAggregator courseAggregator;

        public CourseController(IStorage<string, Account> accountStorage,
            IStorage<int, Course> courseStorage,
            IStorage<int, CourseMember> courseMemberStorage,
            IStorage<int, Announcement> announcementStorage,
            IStorage<int, Homework> homeworkStorage,
            IStorage<string, HomeworkStatus> homeworkStatusStorage,
            CourseAggregator courseAggregator) : base(accountStorage)
        {
            SectionRootPath = CourseRootPath;
            this.courseStorage = courseStorage;
            this.courseMemberStorage = courseMemberStorage;
            this.announcementStorage = announcementStorage;
            this.homeworkStorage = homeworkStorage;
            this.homeworkStatusStorage = homeworkStatusStorage;
            this.courseAggregator = courseAggregator;
        }

        [HttpGet]
        [RequireHttps]
        [Route("/courses")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Courses_Get()
        {
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
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
            return View("Courses", model);
        }

        [HttpPost]
        [RequireHttps]
        [Route("/courses")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Courses_Post()
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
            var course = new Course(account.Username);
            if (!await courseStorage.TryInsert(course))
            {
                return View("Status", ActionStatus.UnknownError);
            }
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
            return RedirectPermanent($"{CourseRootPath}/edit?id={course.Id}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Course_Get(int id)
        {
            if (id <= 0)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var course = await courseStorage.Find(id);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            if (!await CanViewCourse(course))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            return View("Course", new AccountAndObject<Course>(account, course));
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/edit")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> EditCourse_Get(int id)
        {
            if (id <= 0)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var course = await courseStorage.Find(id);
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
            var course = await courseStorage.Find(id);
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
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
            if (courseId <= 0)
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
            if (!await CanViewCourse(course))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            var members = await courseMemberStorage.Select(member => member.CourseId == courseId);
            var model = new CourseAndObject<List<CourseMember>>(account, course, members);
            return View("CourseMembers", model);
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/members/add")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> AddMembers_Get(int courseId)
        {
            if (courseId <= 0)
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
            var course = await courseStorage.Find(courseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
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
            if (courseId <= 0)
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
            var course = await courseStorage.Find(courseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
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
                foreach (var courseMember in courseMembers)
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
            var model = new CourseAndObject<CourseMember>(account, course, courseMember);
            return View("CourseMember", model);
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
            var model = new CourseAndObject<CourseMember>(account, course, courseMember);
            return View("CourseMember", model);
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/announcements")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Announcements_Get(int courseId)
        {
            if (courseId <= 0)
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
            var courseMember = await GetCourseMember(courseId, account.Username);
            if (!await CanViewCourse(course, courseMember))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            var announcements = await announcementStorage.Select(
                announcement => announcement.CourseId == courseId
            );
            courseMember ??= CourseMember.BuildSpectator(courseId, account.Username);
            var model = new CourseMemberAndObject<List<Announcement>>(courseMember, announcements);
            return View("Announcements", model);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/announcements")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Announcements_Post(int courseId)
        {
            if (courseId <= 0)
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
            var courseMember = await GetCourseMember(courseId, account.Username);
            if (courseMember is null || !courseMember.CanEditHomeworks)
            {
                return View("Status", ActionStatus.NoPermission);
            }
            var announcement = new Announcement(courseId);
            if (!await announcementStorage.TryInsert(announcement))
            {
                return View("Status", ActionStatus.UnknownError);
            }
            return RedirectPermanent($"{CourseRootPath}/announcement/edit?id={announcement.Id}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/announcement")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Announcement_Get(int id)
        {
            if (id <= 0)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var announcement = await announcementStorage.Find(id);
            if (announcement is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var course = await courseStorage.Find(announcement.CourseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var courseMember = await GetCourseMember(course.Id, account.Username);
            if (!await CanViewCourse(course, courseMember))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            courseMember ??= CourseMember.BuildSpectator(course.Id, account.Username);
            var model = new CourseMemberAndObject<Announcement>(courseMember, announcement);
            return View("Announcement", model);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/announcement")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Announcement_Post(int id)
        {
            if (id <= 0)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var announcement = await announcementStorage.Find(id);
            if (announcement is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var course = await courseStorage.Find(announcement.CourseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var courseMember = await GetCourseMember(course.Id, account.Username);
            if (courseMember is null || !courseMember.CanEditHomeworks)
            {
                return View("Status", ActionStatus.NoPermission);
            }
            await announcementStorage.TryDeleteValue(announcement);
            return RedirectPermanent($"{CourseRootPath}/announcements?courseId={course.Id}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/announcement" + "/edit")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> EditAnnouncement_Get(int id)
        {
            if (id <= 0)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var announcement = await announcementStorage.Find(id);
            if (announcement is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var course = await courseStorage.Find(announcement.CourseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var courseMember = await GetCourseMember(course.Id, account.Username);
            if (courseMember is null || !courseMember.CanEditHomeworks)
            {
                return View("Status", ActionStatus.NoPermission);
            }
            return View("EditAnnouncement", announcement);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/announcement" + "/edit")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> EditAnnouncement_Post(int id)
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
            var announcement = await announcementStorage.Find(id);
            if (announcement is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var course = await courseStorage.Find(announcement.CourseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var courseMember = await GetCourseMember(course.Id, account.Username);
            if (courseMember is null || !courseMember.CanEditHomeworks)
            {
                return View("Status", ActionStatus.NoPermission);
            }
            var parser = new FormParser(Request.Form);
            var title = DataUtils.GetTrimmed(parser.GetString("title"));
            var content = DataUtils.GetTrimmed(parser.GetString("content"));
            if (title is not null && Announcement.HasTitleValidFormat(title))
            {
                announcement.Title = title;
            }
            if (Announcement.HasContentValidFormat(content))
            {
                announcement.Content = content;
            }
            await announcementStorage.Update(announcement);
            return RedirectPermanent($"{CourseRootPath}/announcement?id={announcement.Id}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/tasks")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Tasks_Get(int courseId)
        {
            if (courseId <= 0)
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
            var courseMember = await GetCourseMember(courseId, account.Username);
            if (!await CanViewCourse(course, courseMember))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            courseMember ??= CourseMember.BuildSpectator(course.Id, account.Username);
            var allPersonalHomework = await GetAllPersonalHomework(courseMember);
            var model = new CourseMemberAndObject<List<PersonalHomework>>(
                courseMember, allPersonalHomework
            );
            return View("Tasks", model);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/tasks")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Tasks_Post(int courseId)
        {
            if (courseId <= 0 || !Request.HasFormContentType)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var parser = new FormParser(Request.Form);
            var actionCode = parser.GetString("actionCode");
            if (actionCode != "add-individual" && actionCode != "add-team")
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
            var courseMember = await GetCourseMember(courseId, account.Username);
            if (courseMember is null || !courseMember.CanEditHomeworks)
            {
                return View("Status", ActionStatus.NoPermission);
            }
            var isTeamwork = actionCode == "add-team";
            var homework = new Homework(courseId, isTeamwork);
            if (!await homeworkStorage.TryInsert(homework))
            {
                return View("Status", ActionStatus.UnknownError);
            }
            return RedirectPermanent($"{CourseRootPath}/task/edit?id={homework.Id}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/task")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Task_Get(int id)
        {
            if (id <= 0)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var homework = await homeworkStorage.Find(id);
            if (homework is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var course = await courseStorage.Find(homework.CourseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var courseMember = await GetCourseMember(course.Id, account.Username);
            if (!await CanViewCourse(course, courseMember))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            HomeworkStatus? homeworkStatus = null;
            if (courseMember is null)
            {
                courseMember ??= CourseMember.BuildSpectator(course.Id, account.Username);
            }
            else
            {
                string? subjectId = null;
                if (homework.IsTeamwork)
                {
                    int? teamId = courseMember.TeamId;
                    if (teamId is not null)
                    {
                        subjectId = Team.BuildTag((int)teamId);
                    }
                }
                else
                {
                    subjectId = account.Username;
                }
                if (subjectId is not null)
                {
                    var uid = HomeworkStatus.BuildUid(homework.Id, subjectId);
                    homeworkStatus = await homeworkStatusStorage.Find(uid);
                }
            }
            homeworkStatus ??= new HomeworkStatus(homework.Id, account.Username);
            var personalHomework = new PersonalHomework(homework, homeworkStatus);
            var model = new CourseMemberAndObject<PersonalHomework>(courseMember, personalHomework);
            return View("Task", model);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/task")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Task_Post(int id)
        {
            if (id <= 0 || !Request.HasFormContentType)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var parser = new FormParser(Request.Form);
            var actionCode = parser.GetString("actionCode");
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var homework = await homeworkStorage.Find(id);
            if (homework is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var courseMember = await GetCourseMember(homework.CourseId, account.Username);
            if (courseMember is null)
            {
                return View("Status", ActionStatus.NoPermission);
            }
            if (actionCode == "delete")
            {
                if (!courseMember.CanEditHomeworks)
                {
                    return View("Status", ActionStatus.NoPermission);
                }
                await homeworkStorage.TryDeleteValue(homework);
                return RedirectPermanent($"{CourseRootPath}/tasks?courseId={homework.CourseId}");
            }
            if (actionCode != "mark-as-submitted" && actionCode != "mark-as-not-submitted")
            {
                return View("Status", ActionStatus.NotSupported);
            }
            string subjectId;
            if (homework.IsTeamwork)
            {
                int? teamId = courseMember.TeamId;
                if (teamId is null)
                {
                    return View("Status", ActionStatus.NotSupported);
                }
                subjectId = Team.BuildTag((int)teamId);
            }
            else
            {
                subjectId = account.Username;
            }
            var uid = HomeworkStatus.BuildUid(homework.Id, subjectId);
            var homeworkStatus = await homeworkStatusStorage.Find(uid);
            bool hasHomeworkStatus = homeworkStatus is not null;
            homeworkStatus ??= new HomeworkStatus(homework.Id, subjectId);
            if (actionCode == "mark-as-submitted")
            {
                homeworkStatus.MarkSumbitted(account.Username);
            }
            else
            {
                homeworkStatus.MarkNotSumbitted();
            }
            if (hasHomeworkStatus || !await homeworkStatusStorage.TryInsert(homeworkStatus))
            {
                await homeworkStatusStorage.Update(homeworkStatus);
            }
            var course = await courseStorage.Find(homework.CourseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var personalHomework = new PersonalHomework(homework, homeworkStatus);
            var model = new CourseMemberAndObject<PersonalHomework>(courseMember, personalHomework);
            return View("Task", model);
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/task" + "/edit")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> EditTask_Get(int id)
        {
            if (id <= 0)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var homework = await homeworkStorage.Find(id);
            if (homework is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var course = await courseStorage.Find(homework.CourseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var courseMember = await GetCourseMember(course.Id, account.Username);
            if (courseMember is null || !courseMember.CanEditHomeworks)
            {
                return View("Status", ActionStatus.NoPermission);
            }
            return View("EditTask", homework);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/task" + "/edit")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> EditTask_Post(int id)
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
            var homework = await homeworkStorage.Find(id);
            if (homework is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var course = await courseStorage.Find(homework.CourseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var courseMember = await GetCourseMember(course.Id, account.Username);
            if (courseMember is null || !courseMember.CanEditHomeworks)
            {
                return View("Status", ActionStatus.NoPermission);
            }
            var parser = new FormParser(Request.Form);
            var title = DataUtils.GetTrimmed(parser.GetString("title"));
            var content = DataUtils.GetTrimmed(parser.GetString("content"));
            var taskUrl = DataUtils.GetTrimmed(parser.GetString("taskUrl"));
            var submitUrl = DataUtils.GetTrimmed(parser.GetString("submitUrl"));
            var extraUrl = DataUtils.GetTrimmed(parser.GetString("extraUrl"));
            var extraUrlLabel = DataUtils.GetTrimmed(parser.GetString("extraUrlLabel"));
            if (title is not null && Announcement.HasTitleValidFormat(title))
            {
                homework.Title = title;
            }
            if (Announcement.HasContentValidFormat(content))
            {
                homework.Content = content;
            }
            if (Course.HasUrlValidFormat(taskUrl))
            {
                homework.TaskUrl = taskUrl;
            }
            if (Course.HasUrlValidFormat(taskUrl))
            {
                homework.SubmitUrl = submitUrl;
            }
            if (Course.HasUrlValidFormat(extraUrl))
            {
                homework.ExtraUrl = extraUrl;
            }
            if (Homework.HasUrlLabelValidFormat(extraUrlLabel))
            {
                homework.ExtraUrlLabel = extraUrlLabel;
            }
            if (parser.TryGetDateTime("deadline", out var deadline))
            {
                homework.Deadline = deadline.AddHours(-DataUtils.MskHourOffset);
            }
            await homeworkStorage.Update(homework);
            return RedirectPermanent($"{CourseRootPath}/task?id={homework.Id}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/marks")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Marks_Get(int taskId)
        {
            if (taskId <= 0)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var homework = await homeworkStorage.Find(taskId);
            if (homework is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var courseMember = await GetCourseMember(homework.CourseId, account.Username);
            if (courseMember is null || !courseMember.CanEvaluateHomeworks)
            {
                return View("Status", ActionStatus.NoPermission);
            }
            var allStatus = await GetAllHomeworkStatus(homework);
            return View("Marks", new HomeworkWithAllStatus(homework, allStatus));
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/marks")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Marks_Post(int taskId)
        {
            if (taskId <= 0 || !Request.HasFormContentType)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var homework = await homeworkStorage.Find(taskId);
            if (homework is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var courseMember = await GetCourseMember(homework.CourseId, account.Username);
            if (courseMember is null || !courseMember.CanEvaluateHomeworks)
            {
                return View("Status", ActionStatus.NoPermission);
            }
            var allStatus = await GetAllHomeworkStatus(homework);
            var parser = new FormParser(Request.Form);
            foreach (var status in allStatus)
            {
                var markKey = $"mark{status.SubjectId}";
                if (parser.GetString(markKey) == "reset")
                {
                    status.ResetEvaluation();
                }
                else if (parser.TryGetInt(markKey, out var mark))
                {
                    status.Evaluate(mark, account.Username);
                }
                else
                {
                    continue;
                }
                if (status.IsSubmitted || !await homeworkStatusStorage.TryInsert(status))
                {
                    await homeworkStatusStorage.Update(status);
                }
            }
            return View("Marks", new HomeworkWithAllStatus(homework, allStatus));
        }

        private async Task<bool> CanViewCourse(Course course)
        {
            var account = await GetAccount();
            if (account is null)
            {
                return false;
            }
            if (account.Role == UserRole.Manager
                || account.Role == UserRole.Teacher && account.Username == course.OwnerUsername)
            {
                return true;
            }
            return await GetCourseMember(course.Id, account.Username) is not null;
        }

        private async Task<bool> CanViewCourse(Course course, CourseMember? courseMember)
        {
            var account = await GetAccount();
            if (account is null)
            {
                return false;
            }
            if (account.Role == UserRole.Manager || account.Username == course.OwnerUsername)
            {
                return true;
            }
            return courseMember is not null;
        }

        private async Task<CourseMember?> GetCourseMember(int courseId, string username)
        {
            var members = await courseMemberStorage.Select(
                    member => member.CourseId == courseId && member.Username == username
            );
            return members.FirstOrDefault();
        }

        private async Task<List<PersonalHomework>> GetAllPersonalHomework(CourseMember courseMember)
        {
            var courseId = courseMember.CourseId;
            var username = courseMember.Username;
            var result = await courseAggregator.GetAllHomeworkWithStatus(courseId, username);
            if (courseMember.TeamId is not null)
            {
                var tag = Team.BuildTag(courseId);
                var allTeamHomework = await courseAggregator.GetAllHomeworkWithStatus(courseId, tag);
                result = result.Concat(allTeamHomework).ToList();
            }
            var allHomework = await homeworkStorage.Select(homework => homework.CourseId == courseId);
            foreach (var homework in allHomework)
            {
                if (result.All(personalHomework => personalHomework.Homework.Id != homework.Id))
                {
                    var homeworkStatus = new HomeworkStatus(homework.Id, username);
                    result.Add(new PersonalHomework(homework, homeworkStatus));
                }
            }
            return result;
        }

        private async Task<List<HomeworkStatus>> GetAllHomeworkStatus(Homework homework)
        {
            List<HomeworkStatus> allStatus;
            if (homework.IsTeamwork)
            {
                // TODO: find all teams and status
                allStatus = new();
            }
            else
            {
                allStatus = await homeworkStatusStorage.Select(
                    homeworkStatus => homeworkStatus.HomeworkId == homework.Id
                );
                var allMembers = await courseMemberStorage.Select(
                    courseMember => courseMember.CourseId == homework.CourseId
                );
                var completers = allMembers.Where(member => member.CanSubmitHomeworks).ToList();
                foreach (var member in completers)
                {
                    if (allStatus.All(status => status.SubjectId != member.Username))
                    {
                        allStatus.Add(new HomeworkStatus(homework.Id, member.Username));
                    }
                }
            }
            return allStatus;
        }
    }
}