using HomemadeLMS.Models;
using HomemadeLMS.Models.Domain;
using HomemadeLMS.Services.Data;
using HomemadeLMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
        private readonly IStorage<string, CourseMember> courseMemberStorage;
        private readonly IStorage<string, HomeworkStatus> homeworkStatusStorage;
        private readonly CourseAggregator courseAggregator;

        public CourseController(IStorage<int, Announcement> announcementStorage,
            IStorage<int, Course> courseStorage,
            IStorage<int, Homework> homeworkStorage,
            IStorage<int, Team> teamStorage,
            IStorage<string, Account> accountStorage,
            IStorage<string, CourseMember> courseMemberStorage,
            IStorage<string, HomeworkStatus> homeworkStatusStorage,
            CourseAggregator courseAggregator) : base(accountStorage)
        {
            SectionRootPath = CourseRootPath;
            this.announcementStorage = announcementStorage;
            this.courseStorage = courseStorage;
            this.homeworkStorage = homeworkStorage;
            this.teamStorage = teamStorage;
            this.courseMemberStorage = courseMemberStorage;
            this.homeworkStatusStorage = homeworkStatusStorage;
            this.courseAggregator = courseAggregator;
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
            var course = await courseStorage.Find(courseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var courseMember = await GetCourseMember(courseId, account.Username);
            if (courseMember is null || !courseMember.IsTeacher)
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
            var course = await courseStorage.Find(announcement.CourseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var courseMember = await GetCourseMember(course.Id, account.Username);
            if (courseMember is null || !courseMember.IsTeacher)
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
            var course = await courseStorage.Find(announcement.CourseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var courseMember = await GetCourseMember(course.Id, account.Username);
            if (courseMember is null || !courseMember.IsTeacher)
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
            var course = await courseStorage.Find(announcement.CourseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var courseMember = await GetCourseMember(course.Id, account.Username);
            if (courseMember is null || !courseMember.IsTeacher)
            {
                return View("Status", ActionStatus.NoPermission);
            }
            await announcementStorage.TryDelete(announcement);
            return RedirectPermanent($"{CourseRootPath}/announcements?courseId={course.Id}");
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
            if (!await CanViewCourse(course))
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
                    await courseMemberStorage.TryDelete(courseMember);
                }
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
            var courseMember = await GetCourseMember(courseId, username);
            if (courseMember is null || !await CanViewCourse(course, courseMember))
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
            var courseMember = await GetCourseMember(courseId, username);
            if (courseMember is null || !course.CanBeEditedBy(account))
            {
                return View("Status", ActionStatus.NotSupported);
            }
            if (new FormParser(Request.Form).TryGetCourseRole("roleCode", out CourseRole role))
            {
                courseMember.Role = role;
                courseMember.TeamId = null;
                await courseMemberStorage.Update(courseMember);
            }
            var model = new CourseAndObject<CourseMember>(account, course, courseMember);
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
            var courseMember = await GetCourseMember(homework.CourseId, account.Username);
            if (courseMember is null || courseMember.IsStudent)
            {
                return View("Status", ActionStatus.NoAccess);
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
            if (courseMember is null || courseMember.IsStudent)
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
            var course = await courseStorage.Find(courseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var courseMember = await GetCourseMember(courseId, account.Username);
            if (courseMember is null || !courseMember.IsTeacher)
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
            var course = await courseStorage.Find(homework.CourseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotFound);
            }
            var courseMember = await GetCourseMember(course.Id, account.Username);
            if (courseMember is null || !courseMember.IsTeacher)
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
            var course = await courseStorage.Find(homework.CourseId);
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var courseMember = await GetCourseMember(course.Id, account.Username);
            if (courseMember is null || !courseMember.IsTeacher)
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
        public async Task<IActionResult> Task_One_Post(int id)
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
                if (!courseMember.IsTeacher)
                {
                    return View("Status", ActionStatus.NoPermission);
                }
                await homeworkStorage.TryDelete(homework);
                var allStatus = await homeworkStatusStorage.Select(status => status.HomeworkId == id);
                foreach (var status in allStatus)
                {
                    await homeworkStatusStorage.TryDelete(status);
                }
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
            var courseMember = await GetCourseMember(courseId, account.Username);
            if (!await CanViewCourse(course, courseMember))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            courseMember ??= CourseMember.BuildSpectator(courseId, account.Username);
            var model = new TeamsVM(course, courseMember);
            var teams = await teamStorage.Select(team => team.CourseId == courseId);
            model.Teams = teams;
            var allMembers = await courseMemberStorage.Select(team => team.CourseId == courseId);
            await FixTeamIds(allMembers, teams);
            model.MembersWithoutTeam = allMembers.Where(
                member => member.TeamId is null && member.IsStudent
            ).ToList();
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
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var courseMember = await GetCourseMember(course.Id, account.Username);
            if (courseMember is null || !courseMember.CanCreateTeam(course))
            {
                return View("Status", ActionStatus.NoPermission);
            }
            var team = new Team(courseId);
            if (courseMember.IsStudent)
            {
                team.LeaderUsername = account.Username;
            }
            if (!await teamStorage.TryInsert(team))
            {
                return View("Status", ActionStatus.UnknownError);
            }
            courseMember.TeamId = team.Id;
            await courseMemberStorage.Update(courseMember);
            return RedirectPermanent($"{CourseRootPath}/team/edit?id={team.Id}");
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
            var courseMember = await GetCourseMember(course.Id, account.Username);
            if (courseMember is null || !await CanViewCourse(course, courseMember))
            {
                return View("Status", ActionStatus.NoAccess);
            }
            var teamMembers = await courseMemberStorage.Select(member => member.TeamId == team.Id);
            var model = new TeamVM(course, courseMember, team);
            foreach (var member in teamMembers)
            {
                var memberAccount = await accountStorage.Find(member.Username);
                model.Members.Add(new(member, memberAccount));
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
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            if (!course.HasTeams)
            {
                return RedirectPermanent($"{CourseRootPath}/course?id={course.Id}");
            }
            var courseMember = await GetCourseMember(course.Id, account.Username);
            if (courseMember is null)
            {
                return View("Status", ActionStatus.NoPermission);
            }
            if (actionCode == "join" || actionCode == "leave")
            {
                courseMember.TeamId = actionCode == "join" ? team.Id : null;
                await courseMemberStorage.Update(courseMember);
                return RedirectPermanent($"{CourseRootPath}/team?id={team.Id}");
            }
            else
            {
                if (!courseMember.CanEditTeam(course, team))
                {
                    return View("Status", ActionStatus.NoPermission);
                }
                var teamMembers = await courseMemberStorage.Select(member => member.TeamId == team.Id);
                foreach (var member in teamMembers)
                {
                    member.TeamId = null;
                    await courseMemberStorage.Update(member);
                }
                await teamStorage.TryDelete(team);
            }
            return RedirectPermanent($"{CourseRootPath}/teams?courseId={course.Id}");
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
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            var courseMember = await GetCourseMember(course.Id, account.Username);
            if (courseMember is null || !courseMember.CanEditTeam(course, team))
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
            if (course is null)
            {
                return View("Status", ActionStatus.NotSupported);
            }
            if (!course.HasTeams)
            {
                return RedirectPermanent($"{CourseRootPath}/course?id={course.Id}");
            }
            var courseMember = await GetCourseMember(course.Id, account.Username);
            if (courseMember is null || !courseMember.CanEditTeam(course, team))
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
                var otherMember = await courseMemberStorage.Find(uid);
                if (otherMember is not null && otherMember.TeamId is null)
                {
                    otherMember.TeamId = teamId;
                    await courseMemberStorage.Update(otherMember);
                }
            }
            if (actionCode == "remove" && Account.HasUsernameValidFormat(removedUsername))
            {
                var uid = CourseMember.BuildUid(course.Id, removedUsername);
                var otherMember = await courseMemberStorage.Find(uid);
                if (otherMember is not null && otherMember.TeamId == teamId)
                {
                    otherMember.TeamId = null;
                    await courseMemberStorage.Update(otherMember);
                }
            }
            return RedirectPermanent($"{CourseRootPath}/team/members?teamId={team.Id}");
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
            var courseMember = await GetCourseMember(course.Id, account.Username);
            if (courseMember is null || !courseMember.CanEditTeam(course, team))
            {
                return View("Status", ActionStatus.NoPermission);
            }
            var model = new TeamEditionVM(team);
            var teamMembers = await courseMemberStorage.Select(member => member.TeamId == team.Id);
            model.Members = teamMembers;
            await FixLeaderUsername(team, teamMembers);
            return View(isForMembers ? "EditTeamMembers" : "EditTeam", model);
        }

        private async Task<bool> CanViewCourse(Course course)
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
                var allMembers = await courseMemberStorage.Select(
                    courseMember => courseMember.CourseId == homework.CourseId
                );
                var completers = allMembers.Where(member => member.IsStudent).ToList();
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

        private async Task<List<PersonalHomework>> GetAllPersonalHomework(CourseMember courseMember)
        {
            var courseId = courseMember.CourseId;
            var username = courseMember.Username;
            var result = await courseAggregator.GetAllHomeworkWithStatus(courseId, username);
            var teamId = courseMember.TeamId;
            if (teamId is not null)
            {
                var tag = Team.BuildTag((int)teamId);
                var allTeamHomework = await courseAggregator.GetAllHomeworkWithStatus(courseId, tag);
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

        private async Task<CourseMember?> GetCourseMember(int courseId, string username)
        {
            var members = await courseMemberStorage.Select(
                    member => member.CourseId == courseId && member.Username == username
            );
            return members.FirstOrDefault();
        }

        private async Task FixLeaderUsername(Team team, List<CourseMember> teamMembers)
        {
            if (teamMembers.Any()
                && teamMembers.All(member => member.Username != team.LeaderUsername))
            {
                team.LeaderUsername = teamMembers.First().Username;
                await teamStorage.Update(team);
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
                if (courseTeams.All(team => team.Id != member.TeamId))
                {
                    member.TeamId = null;
                    await courseMemberStorage.Update(member);
                }
            }
        }
    }
}