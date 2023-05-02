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
        private readonly IStorage<int, Announcement> announcementStorage;
        private readonly IStorage<int, Course> courseStorage;
        private readonly IStorage<int, Homework> homeworkStorage;
        private readonly IStorage<int, Team> teamStorage;
        private readonly IStorage<string, CourseMember> memberStorage;
        private readonly IStorage<string, HomeworkStatus> homeworkStatusStorage;
        private readonly IStorage<string, RoleTestResult> testResultStorage;
        private readonly EntityAggregator entityAggregator;

        public CourseController(IStorage<int, Announcement> announcementStorage,
            IStorage<int, Course> courseStorage,
            IStorage<int, Homework> homeworkStorage,
            IStorage<int, Team> teamStorage,
            IStorage<string, Account> accountStorage,
            IStorage<string, CourseMember> memberStorage,
            IStorage<string, HomeworkStatus> homeworkStatusStorage,
            IStorage<string, RoleTestResult> testResultStorage,
            EntityAggregator entityAggregator) : base(accountStorage)
        {
            SectionRootPath = CourseRootPath;
            this.announcementStorage = announcementStorage;
            this.courseStorage = courseStorage;
            this.homeworkStorage = homeworkStorage;
            this.teamStorage = teamStorage;
            this.memberStorage = memberStorage;
            this.homeworkStatusStorage = homeworkStatusStorage;
            this.testResultStorage = testResultStorage;
            this.entityAggregator = entityAggregator;
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/announcements")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Announcement_All_Get(int courseId)
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
            var member = await GetSelfCourseMember(courseId);
            if (!await CanViewCourse(course, member))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            var announcements = await announcementStorage.Select(item => item.CourseId == courseId);
            var model = new MemberAndObject<List<Announcement>>(member, announcements);
            return View("Announcements", model);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/announcements")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Announcement_All_Post(int courseId)
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
            // Should be used if course deletion is not 100% clean.
            //var course = await courseStorage.Find(courseId);
            //if (course is null)
            //{
            //    return View("Status", ActionStatus.NotSupported);
            //}
            var member = await GetSelfCourseMember(courseId);
            if (!member.IsTeacher)
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
        [Route(CourseRootPath + "/announcement" + "/edit")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Announcement_Edit_Get(int id)
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
            // Should be used if course deletion is not 100% clean.
            //var course = await courseStorage.Find(announcement.CourseId);
            //if (course is null)
            //{
            //    return View("status", ActionStatus.NotFound);
            //}
            var member = await GetSelfCourseMember(announcement.CourseId);
            if (!member.IsTeacher)
            {
                return View("Status", ActionStatus.NoPermission);
            }
            return View("EditAnnouncement", announcement);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/announcement" + "/edit")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Announcement_Edit_Post(int id)
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
            // Should be used if course deletion is not 100% clean.
            //var course = await courseStorage.Find(announcement.CourseId);
            //if (course is null)
            //{
            //    return View("Status", ActionStatus.NotSupported);
            //}
            var member = await GetSelfCourseMember(announcement.CourseId);
            if (!member.IsTeacher)
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
        [Route(CourseRootPath + "/announcement")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Announcement_One_Get(int id)
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
            var member = await GetSelfCourseMember(announcement.CourseId);
            if (!await CanViewCourse(course, member))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            var model = new MemberAndObject<Announcement>(member, announcement);
            return View("Announcement", model);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/announcement")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Announcement_One_Post(int id)
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
            var courseId = announcement.CourseId;
            // Should be used if course deletion is not 100% clean.
            //var course = await courseStorage.Find(announcement.CourseId);
            //if (course is null)
            //{
            //    return View("Status", ActionStatus.NotSupported);
            //}
            var member = await GetSelfCourseMember(courseId);
            if (!member.IsTeacher)
            {
                return View("Status", ActionStatus.NoPermission);
            }
            await announcementStorage.TryDelete(announcement);
            return RedirectPermanent($"{CourseRootPath}/announcements?courseId={courseId}");
        }

        [HttpGet]
        [RequireHttps]
        [Route("/courses")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Course_All_Get()
        {
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            List<Course> courses;
            if (account.Role == UserRole.Manager)
            {
                courses = await courseStorage.Select(_ => true);
            }
            else
            {
                courses = await entityAggregator.GetUserCourses(account.Username);
                var ownedCourses = await courseStorage.Select(
                    course => course.OwnerUsername == account.Username
                );
                bool IsSelected(int courseId) => courses.Any(course => course.Id == courseId);
                var notSelectedCourses = ownedCourses.Where(course => !IsSelected(course.Id));
                courses = courses.Concat(notSelectedCourses).ToList();
            }
            var model = new AccountAndObject<List<Course>>(account, courses);
            return View("Courses", model);
        }

        [HttpPost]
        [RequireHttps]
        [Route("/courses")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Course_All_Post()
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
            if (account.Role != UserRole.Manager)
            {
                var member = new CourseMember(course.Id, account.Username, CourseRole.Teacher);
                if (!await memberStorage.TryInsert(member))
                {
                    return View("Status", ActionStatus.UnknownError);
                }
            }
            return RedirectPermanent($"{CourseRootPath}/edit?id={course.Id}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/edit")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Course_Edit_Get(int id)
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
        public async Task<IActionResult> Course_Edit_Post(int id)
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
            course.HasTeams = parser.IsChecked("hasTeams");
            course.IsTeamStateLocked = parser.IsChecked("isTeamStateLocked");
            course.HasRoleTestResults = parser.IsChecked("hasRoleTestResults");
            await courseStorage.Update(course);
            return RedirectPermanent($"{CourseRootPath}?id={course.Id}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Course_One_Get(int id)
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
            var member = await GetSelfCourseMember(course.Id);
            if (!await CanViewCourse(course, member))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            return View("Course", new AccountAndObject<Course>(account, course));
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/members")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> CourseMember_All_Get(int courseId)
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
            var member = await GetSelfCourseMember(courseId);
            if (!await CanViewCourse(course, member))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            var allMemberInfo = await entityAggregator.GetCourseMemberInfo(courseId);
            var courseMembers = await GetCourseMembers(courseId);
            foreach (var courseMember in courseMembers)
            {
                if (allMemberInfo.All(info => info.Member.Username != courseMember.Username))
                {
                    allMemberInfo.Add(new(courseMember));
                }
            }
            var model = new CourseAndObject<List<MemberInfo>>(account, course, allMemberInfo);
            return View("CourseMembers", model);
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/members/add")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> CourseMember_Add_Get(int courseId)
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
        public async Task<IActionResult> CourseMember_Add_Post(int courseId)
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
                var member = new CourseMember(courseId, username, CourseRole.Student);
                if (await memberStorage.TryInsert(member))
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
        public async Task<IActionResult> CourseMember_Remove_Get(int courseId)
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
        public async Task<IActionResult> CourseMember_Remove_Post(int courseId)
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
                var uid = CourseMember.BuildUid(courseId, username);
                var member = await memberStorage.Find(uid);
                if (member is null)
                {
                    model.AlreadyRemovedUsernames.Add(username);
                    continue;
                }
                await memberStorage.TryDelete(member);
                model.RemovedUsernames.Add(username);
            }
            return View("MemberChangelog", model);
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/member")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> CourseMember_One_Get(int courseId, string? username)
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
            var uid = CourseMember.BuildUid(courseId, username);
            var otherMember = await memberStorage.Find(uid);
            if (otherMember is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var selfMember = await GetSelfCourseMember(courseId);
            if (!await CanViewCourse(course, selfMember))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            var otherAccount = await accountStorage.Find(otherMember.Username);
            var memberInfo = new MemberInfo(otherMember, otherAccount);
            var model = new CourseAndObject<MemberInfo>(account, course, memberInfo);
            return View("CourseMember", model);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/member")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> CourseMember_One_Post(int courseId, string? username)
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
            if (!course.CanBeEditedBy(account))
            {
                return View("Status", ActionStatus.NoPermission);
            }
            var uid = CourseMember.BuildUid(courseId, username);
            var otherMember = await memberStorage.Find(uid);
            if (otherMember is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            if (new FormParser(Request.Form).TryGetCourseRole("roleCode", out CourseRole role))
            {
                otherMember.Role = role;
                otherMember.TeamId = null;
                await memberStorage.Update(otherMember);
            }
            var otherAccount = await accountStorage.Find(otherMember.Username);
            var memberInfo = new MemberInfo(otherMember, otherAccount);
            var model = new CourseAndObject<MemberInfo>(account, course, memberInfo);
            return View("CourseMember", model);
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
            // Should be used if course deletion is not 100% clean.
            //var course = await courseStorage.Find(homework.CourseId);
            //if (course is null)
            //{
            //    return View("Status", ActionStatus.NotFound);
            //}
            var member = await GetSelfCourseMember(homework.CourseId);
            if (!member.IsAssistantOrTeacher)
            {
                return View("Status", ActionStatus.NoAccess);
            }
            var allStatus = await GetAllHomeworkStatus(homework);
            return View("Marks", new HomeworkInfo(homework, allStatus));
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
            // Should be used if course deletion is not 100% clean.
            //var course = await courseStorage.Find(homework.CourseId);
            //if (course is null)
            //{
            //    return View("Status", ActionStatus.NotSupported);
            //}
            var member = await GetSelfCourseMember(homework.CourseId);
            if (!member.IsAssistantOrTeacher)
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
            return View("Marks", new HomeworkInfo(homework, allStatus));
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/tasks")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Task_All_Get(int courseId)
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
            var member = await GetSelfCourseMember(courseId);
            if (!await CanViewCourse(course, member))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            var allPersonalHomework = await GetAllPersonalHomework(member);
            var model = new MemberAndAllPersonalHomework(member, allPersonalHomework, course);
            return View("Tasks", model);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/tasks")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Task_All_Post(int courseId)
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
            // Should be used if course deletion is not 100% clean.
            //var course = await courseStorage.Find(courseId);
            //if (course is null)
            //{
            //    return View("Status", ActionStatus.NotSupported);
            //}
            var member = await GetSelfCourseMember(courseId);
            if (!member.IsTeacher)
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
        [Route(CourseRootPath + "/task" + "/edit")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Task_Edit_Get(int id)
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
            // Should be used if course deletion is not 100% clean.
            //var course = await courseStorage.Find(homework.CourseId);
            //if (course is null)
            //{
            //    return View("Status", ActionStatus.NotFound);
            //}
            var member = await GetSelfCourseMember(homework.CourseId);
            if (!member.IsTeacher)
            {
                return View("Status", ActionStatus.NoPermission);
            }
            return View("EditTask", homework);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/task" + "/edit")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Task_Edit_Post(int id)
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
            // Should be used if course deletion is not 100% clean.
            //var course = await courseStorage.Find(homework.CourseId);
            //if (course is null)
            //{
            //    return View("Status", ActionStatus.NotSupported);
            //}
            var member = await GetSelfCourseMember(homework.CourseId);
            if (!member.IsTeacher)
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
        [Route(CourseRootPath + "/task")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Task_One_Get(int id)
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
            var member = await GetSelfCourseMember(homework.CourseId);
            if (!await CanViewCourse(course, member))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            HomeworkStatus? homeworkStatus = null;
            if (!member.IsStranger)
            {
                string? subjectId = null;
                if (homework.IsTeamwork)
                {
                    int? teamId = member.TeamId;
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
            var model = new MemberAndObject<PersonalHomework>(member, personalHomework);
            return View("Task", model);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/task")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Task_One_Post(int id)
        {
            if (id <= 0 || !Request.HasFormContentType)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var parser = new FormParser(Request.Form);
            var actionCode = parser.GetString("actionCode");
            var allCodes = new string[] { "delete", "mark-as-submitted", "mark-as-not-submitted" };
            if (!allCodes.Contains(actionCode))
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
            // Should be used if course deletion is not 100 % clean.
            //var course = await courseStorage.Find(homework.CourseId);
            //if (course is null)
            //{
            //    return View("Status", ActionStatus.NotSupported);
            //}
            var member = await GetSelfCourseMember(homework.CourseId);
            if (actionCode == "delete")
            {
                if (!member.IsTeacher)
                {
                    return View("Status", ActionStatus.NoPermission);
                }
                await homeworkStorage.TryDelete(homework);
                var allStatus = await homeworkStatusStorage.Select(item => item.HomeworkId == id);
                foreach (var status in allStatus)
                {
                    await homeworkStatusStorage.TryDelete(status);
                }
                return RedirectPermanent($"{CourseRootPath}/tasks?courseId={homework.CourseId}");
            }
            string subjectId;
            if (homework.IsTeamwork)
            {
                int? teamId = member.TeamId;
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
                homeworkStatus.MarkAsSumbitted(account.Username);
            }
            else
            {
                homeworkStatus.MarkAsNotSumbitted();
            }
            if (hasHomeworkStatus || !await homeworkStatusStorage.TryInsert(homeworkStatus))
            {
                await homeworkStatusStorage.Update(homeworkStatus);
            }
            var personalHomework = new PersonalHomework(homework, homeworkStatus);
            var model = new MemberAndObject<PersonalHomework>(member, personalHomework);
            return View("Task", model);
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/teams")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Team_All_Get(int courseId)
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
            if (!course.HasTeams)
            {
                return RedirectPermanent($"{CourseRootPath}?id={courseId}");
            }
            var member = await GetSelfCourseMember(courseId);
            if (!await CanViewCourse(course, member))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            var teams = await teamStorage.Select(team => team.CourseId == courseId);
            var members = await GetCourseMembers(courseId);
            await FixTeamIds(members, teams);
            var singleStudents
                = members.Where(member => member.TeamId is null && member.IsStudent).ToList();
            var allCourseInfo = await entityAggregator.GetCourseMemberInfo(courseId);
            allCourseInfo = allCourseInfo.Where(
                info => info.Member.TeamId is null && info.Member.IsStudent
            ).ToList();
            foreach (var student in singleStudents)
            {
                if (allCourseInfo.All(info => info.Member.Username != student.Username))
                {
                    allCourseInfo.Add(new(student));
                }
            }
            var model = new TeamsVM(course, member, teams, allCourseInfo);
            return View("Teams", model);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/teams")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Team_All_Post(int courseId)
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
            if (course is null || !course.HasTeams)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var member = await GetSelfCourseMember(courseId);
            if (!member.CanCreateTeam(course))
            {
                return View("Status", ActionStatus.NoPermission);
            }
            var team = new Team(courseId);
            if (member.IsStudent)
            {
                team.LeaderUsername = account.Username;
            }
            if (!await teamStorage.TryInsert(team))
            {
                return View("Status", ActionStatus.UnknownError);
            }
            if (member.IsStudent)
            {
                member.TeamId = team.Id;
                await memberStorage.Update(member);
            }
            return RedirectPermanent($"{CourseRootPath}/team/edit?id={team.Id}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/team" + "/edit")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Team_Edit_Get(int id)
        {
            return await Team_Edit_Get_Result(id, isForMembers: false);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/team" + "/edit")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Team_Edit_Post(int id)
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
            var team = await teamStorage.Find(id);
            if (team is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var course = await courseStorage.Find(team.CourseId);
            if (course is null || !course.HasTeams)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var member = await GetSelfCourseMember(course.Id);
            if (!member.CanEditTeam(course, team))
            {
                return View("Status", ActionStatus.NoPermission);
            }
            var parser = new FormParser(Request.Form);
            var name = DataUtils.GetTrimmed(parser.GetString("name"));
            var leaderUsername = Account.GetUsername(parser.GetString("leaderUsername"));
            if (name is not null && Team.HasNameValidFormat(name))
            {
                team.Name = name;
            }
            if (Account.HasUsernameValidFormat(leaderUsername))
            {
                team.LeaderUsername = leaderUsername;
            }
            await teamStorage.Update(team);
            return RedirectPermanent($"{CourseRootPath}/team?id={team.Id}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/team" + "/members")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Team_EditMembers_Get(int teamId)
        {
            return await Team_Edit_Get_Result(teamId, isForMembers: true);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/team" + "/members")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Team_EditMembers_Post(int teamId)
        {
            if (teamId <= 0 || !Request.HasFormContentType)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var team = await teamStorage.Find(teamId);
            if (team is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var course = await courseStorage.Find(team.CourseId);
            if (course is null || !course.HasTeams)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var member = await GetSelfCourseMember(course.Id);
            if (!member.CanEditTeam(course, team))
            {
                return View("Status", ActionStatus.NoPermission);
            }
            var parser = new FormParser(Request.Form);
            var actionCode = parser.GetString("actionCode");
            var addedUsername = DataUtils.GetTrimmed(parser.GetString("addedUsername"));
            var removedUsername = DataUtils.GetTrimmed(parser.GetString("removedUsername"));
            addedUsername ??= string.Empty;
            removedUsername ??= string.Empty;
            if (actionCode != "add" && actionCode != "remove")
            {
                return View("Status", ActionStatus.NotSupported);
            }
            if (actionCode == "add" && Account.HasUsernameValidFormat(addedUsername))
            {
                var uid = CourseMember.BuildUid(course.Id, addedUsername);
                var otherMember = await memberStorage.Find(uid);
                if (otherMember is not null && otherMember.TeamId is null)
                {
                    otherMember.TeamId = teamId;
                    await memberStorage.Update(otherMember);
                }
            }
            if (actionCode == "remove" && Account.HasUsernameValidFormat(removedUsername))
            {
                var uid = CourseMember.BuildUid(course.Id, removedUsername);
                var otherMember = await memberStorage.Find(uid);
                if (otherMember is not null && otherMember.IsInTeam(team))
                {
                    otherMember.TeamId = null;
                    await memberStorage.Update(otherMember);
                }
            }
            return RedirectPermanent($"{CourseRootPath}/team/members?teamId={team.Id}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/team")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Team_One_Get(int id)
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
            var team = await teamStorage.Find(id);
            if (team is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var course = await courseStorage.Find(team.CourseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            if (!course.HasTeams)
            {
                return RedirectPermanent($"{CourseRootPath}?id={course.Id}");
            }
            var member = await GetSelfCourseMember(course.Id);
            if (!await CanViewCourse(course, member))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            var teamMembers = await GetTeamMembers(team.Id);
            var model = new TeamVM(course, member, team);
            foreach (var otherMember in teamMembers)
            {
                var otherAccount = await accountStorage.Find(otherMember.Username);
                model.AllMemberInfo.Add(new(otherMember, otherAccount));
            }
            await FixLeaderUsername(team, teamMembers);
            return View("Team", model);
        }

        [HttpPost]
        [RequireHttps]
        [Route(CourseRootPath + "/team")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Team_One_Post(int id)
        {
            if (id <= 0)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var parser = new FormParser(Request.Form);
            var actionCode = parser.GetString("actionCode");
            if (actionCode != "join" && actionCode != "leave" && actionCode != "delete")
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var team = await teamStorage.Find(id);
            if (team is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var course = await courseStorage.Find(team.CourseId);
            if (course is null || !course.HasTeams)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            if (!course.HasTeams)
            {
                return RedirectPermanent($"{CourseRootPath}/course?id={course.Id}");
            }
            var member = await GetSelfCourseMember(course.Id);
            if (actionCode == "join" || actionCode == "leave")
            {
                if (!member.CanChangeTeam(course))
                {
                    return View("Status", ActionStatus.NotSupported);
                }
                member.TeamId = actionCode == "join" ? team.Id : null;
                await memberStorage.Update(member);
                return RedirectPermanent($"{CourseRootPath}/team?id={team.Id}");
            }
            else
            {
                // actionCode == "delete"
                if (!member.CanEditTeam(course, team))
                {
                    return View("Status", ActionStatus.NoPermission);
                }
                var teamMembers = await GetTeamMembers(team.Id);
                foreach (var teamMember in teamMembers)
                {
                    teamMember.TeamId = null;
                    await memberStorage.Update(member);
                }
                await teamStorage.TryDelete(team);
            }
            return RedirectPermanent($"{CourseRootPath}/teams?courseId={course.Id}");
        }

        [HttpGet]
        [RequireHttps]
        [Route(CourseRootPath + "/team" + "/roles")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> TeamRoles_Get(int teamId)
        {
            if (teamId <= 0)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var team = await teamStorage.Find(teamId);
            if (team is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var course = await courseStorage.Find(team.CourseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            if (!course.HasTeams || !course.HasRoleTestResults)
            {
                return RedirectPermanent($"{CourseRootPath}?id={course.Id}");
            }
            var member = await GetSelfCourseMember(course.Id);
            if (!member.CanSeeRoleResults(team))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            var teamMembers = await GetTeamMembers(team.Id);
            var model = new TeamRoles(team);
            foreach (var otherMember in teamMembers)
            {
                var otherAccount = await accountStorage.Find(otherMember.Username);
                var testResult = await testResultStorage.Find(otherMember.Username);
                model.AllMemberRoles.Add(new(new(otherMember, otherAccount), testResult));
            }
            return View("TeamRoles", model);
        }

        private async Task<IActionResult> Team_Edit_Get_Result(int teamId, bool isForMembers)
        {
            if (teamId <= 0)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var account = await GetAccount();
            if (account is null)
            {
                return RedirectPermanent(SignInPath);
            }
            var team = await teamStorage.Find(teamId);
            if (team is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var course = await courseStorage.Find(team.CourseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            if (!course.HasTeams)
            {
                return RedirectPermanent($"{CourseRootPath}?id={course.Id}");
            }
            var member = await GetSelfCourseMember(course.Id);
            if (!member.CanEditTeam(course, team))
            {
                return View("Status", ActionStatus.NoPermission);
            }
            var model = new TeamEditionVM(team);
            var teamMembers = await GetTeamMembers(team.Id);
            model.Members = teamMembers;
            await FixLeaderUsername(team, teamMembers);
            return View(isForMembers ? "EditTeamMembers" : "EditTeam", model);
        }

        private async Task FixLeaderUsername(Team team, List<CourseMember> teamMembers)
        {
            if (teamMembers.All(member => !member.IsLeader(team)))
            {
                if (teamMembers.Any())
                {
                    team.LeaderUsername = teamMembers.First().Username;
                    await teamStorage.Update(team);
                }
                else if (team.LeaderUsername is not null)
                {
                    team.LeaderUsername = null;
                    await teamStorage.Update(team);
                }
            }
        }

        private async Task FixTeamIds(List<CourseMember> courseMembers, List<Team> courseTeams)
        {
            foreach (var member in courseMembers)
            {
                if (!member.IsStudent || member.TeamId is null)
                {
                    continue;
                }
                if (courseTeams.All(team => !member.IsInTeam(team)))
                {
                    member.TeamId = null;
                    await memberStorage.Update(member);
                }
            }
        }

        private async Task<bool> CanViewCourse(Course course, CourseMember member)
        {
            var account = await GetAccount();
            if (account is null)
            {
                throw new NotSupportedException("Request maker must be authorized.");
            }
            if (account.Role == UserRole.Manager || account.Username == course.OwnerUsername)
            {
                return true;
            }
            return !member.IsStranger;
        }

        private async Task<CourseMember> GetSelfCourseMember(int courseId)
        {
            var account = await GetAccount();
            if (account is null)
            {
                throw new NotSupportedException("Request maker must be authorized.");
            }
            var uid = CourseMember.BuildUid(courseId, account.Username);
            var member = await memberStorage.Find(uid);
            member ??= CourseMember.BuildStranger(courseId, account.Username);
            return member;
        }

        private async Task<List<CourseMember>> GetCourseMembers(int courseId)
            => await memberStorage.Select(member => member.CourseId == courseId);

        private async Task<List<CourseMember>> GetTeamMembers(int teamId)
        {
            var members = await memberStorage.Select(member => member.TeamId == teamId);
            return members.Where(member => member.IsStudent).ToList();
        }

        private async Task<List<HomeworkStatus>> GetAllHomeworkStatus(Homework homework)
        {
            List<HomeworkStatus> allStatus = await homeworkStatusStorage.Select(
                homeworkStatus => homeworkStatus.HomeworkId == homework.Id
            );
            if (homework.IsTeamwork)
            {
                var allTeams = await teamStorage.Select(team => team.CourseId == homework.CourseId);
                foreach (var team in allTeams)
                {
                    if (allStatus.All(status => status.SubjectId != team.Tag))
                    {
                        allStatus.Add(new HomeworkStatus(homework.Id, team.Tag));
                    }
                }
            }
            else
            {
                var courseMembers = await GetCourseMembers(homework.CourseId);
                var students = courseMembers.Where(member => member.IsStudent).ToList();
                foreach (var member in students)
                {
                    if (allStatus.All(status => status.SubjectId != member.Username))
                    {
                        allStatus.Add(new HomeworkStatus(homework.Id, member.Username));
                    }
                }
            }
            return allStatus;
        }

        private async Task<List<PersonalHomework>> GetAllPersonalHomework(CourseMember member)
        {
            if (!member.IsStudent)
            {
                var allCourseHomework = await homeworkStorage.Select(
                    homework => homework.CourseId == member.CourseId
                );
                return allCourseHomework.Select(
                    homework => new PersonalHomework(homework, new(homework.Id, member.Username))
                ).ToList();
            }
            var courseId = member.CourseId;
            var username = member.Username;
            var result = await entityAggregator.GetAllPersonalHomework(courseId, username);
            var teamId = member.TeamId;
            if (teamId is not null)
            {
                var tag = Team.BuildTag((int)teamId);
                var allTeamHomework = await entityAggregator.GetAllPersonalHomework(courseId, tag);
                result = result.Concat(allTeamHomework).ToList();
            }
            var allHomework = await homeworkStorage.Select(homework => homework.CourseId == courseId);
            foreach (var homework in allHomework)
            {
                var subjectId = username;
                if (homework.IsTeamwork)
                {
                    if (teamId is null)
                    {
                        continue;
                    }
                    subjectId = Team.BuildTag((int)teamId);
                }
                if (result.All(personalHomework => personalHomework.Homework.Id != homework.Id))
                {
                    var homeworkStatus = new HomeworkStatus(homework.Id, subjectId);
                    result.Add(new PersonalHomework(homework, homeworkStatus));
                }
            }
            return result;
        }
    }
}