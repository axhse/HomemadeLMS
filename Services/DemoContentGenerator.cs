using HomemadeLMS.Environment;
using HomemadeLMS.Models;
using HomemadeLMS.Models.Domain;
using HomemadeLMS.Services.Data;

namespace HomemadeLMS.Services
{
    public class DemoContentGenerator
    {
        private readonly Random randomGenerator;
        private readonly ILogger logger;
        private readonly IStorage<int, Announcement> announcementStorage;
        private readonly IStorage<int, Course> courseStorage;
        private readonly IStorage<int, Homework> homeworkStorage;
        private readonly IStorage<int, Team> teamStorage;
        private readonly IStorage<string, Account> accountStorage;
        private readonly IStorage<string, CourseMember> memberStorage;
        private readonly IStorage<string, HomeworkStatus> homeworkStatusStorage;
        private readonly IStorage<string, RoleTestResult> testResultStorage;

        public DemoContentGenerator()
        {
            randomGenerator = new Random(DateTime.UtcNow.Millisecond);
            logger = LoggerBuilder.Build(nameof(DemoContentGenerator));

            announcementStorage = new Storage<int, Announcement>(new AnnouncementContext());
            courseStorage = new Storage<int, Course>(new CourseContext());
            homeworkStorage = new Storage<int, Homework>(new HomeworkContext());
            teamStorage = new Storage<int, Team>(new TeamContext());
            accountStorage = new Storage<string, Account>(new AccountContext());
            memberStorage = new Storage<string, CourseMember>(new CourseMemberContext());
            homeworkStatusStorage = new Storage<string, HomeworkStatus>(new HomeworkStatusContext());
            testResultStorage = new Storage<string, RoleTestResult>(new RoleTestResultContext());
        }

        public void CleanAllContent()
        {
            logger.LogInformation("Content clean started.");
            TruncateStorage(announcementStorage);
            TruncateStorage(courseStorage);
            TruncateStorage(homeworkStorage);
            TruncateStorage(teamStorage);
            TruncateStorage(accountStorage);
            TruncateStorage(memberStorage);
            TruncateStorage(homeworkStatusStorage);
            TruncateStorage(testResultStorage);
            logger.LogInformation("Content clean is done.");
        }

        public void GenerateContent()
        {
            logger.LogInformation("Content generation started.");

            var password = "88888888";
            var courseOwnerUsername = "teacher1@hse.ru";
            var yearEnd = new DateTime(DateTime.Now.Year, 12, 31, 23, 59, 59);
            yearEnd = yearEnd.AddHours(-DataUtils.MskHourOffset);

            for (int index = 1; index <= 5; index++)
            {
                var manager = new Account($"manager{index}@hse.ru", UserRole.Manager)
                {
                    Name = $"Менеджер {index}",
                    TelegramUsername = $"manaGGer{index}"
                };
                manager.SetPassword(password);
                AssertIsCompleted(accountStorage.TryInsert(manager).Result);
            }

            logger.LogInformation("Manager accounts are created.");

            for (int index = 1; index <= 5; index++)
            {
                var teacher = new Account($"teacher{index}@hse.ru", UserRole.Teacher)
                {
                    Name = $"Преподаватель {index}",
                    TelegramUsername = $"teacher{index}{index}{index}"
                };
                teacher.SetPassword(password);
                AssertIsCompleted(accountStorage.TryInsert(teacher).Result);
            }
            for (int index = 1; index <= 5; index++)
            {
                var assistant = new Account($"assistant{index}@edu.hse.ru", UserRole.Student)
                {
                    Name = $"Ассистент {index}",
                    TelegramUsername = $"helper{index}"
                };
                assistant.SetPassword(password);
                AssertIsCompleted(accountStorage.TryInsert(assistant).Result);
            }

            logger.LogInformation("Teacher and assistant accounts are created.");

            var studentNames = new string[] { "Иван", "Василий", "Петр", "Михаил", "Richard" };
            var studentLastNames = new string[] { "Иванов", "Васильев", "Петров", "Михайлов" };
            var studentNameCodes = new string[] { "ivan", "vasily", "petr", "mikhayl", "richard" };
            var studentLastNameCodes = new string[] { "ivanov", "vasilev", "petrov", "mikhaylov" };
            for (int nameIndex = 0; nameIndex < 5; nameIndex++)
            {
                for (int lastNameIndex = 0; lastNameIndex < 4; lastNameIndex++)
                {
                    var username = $"{studentNameCodes[nameIndex][0]}{studentLastNameCodes[lastNameIndex]}@edu.hse.ru";
                    var student = new Account(username, UserRole.Student)
                    {
                        Name = $"{studentNames[nameIndex]} {studentLastNames[lastNameIndex]}",
                        TelegramUsername = $"{studentNameCodes[nameIndex]}{studentLastNameCodes[lastNameIndex][..3]}"
                    };
                    student.SetPassword(password);
                    AssertIsCompleted(accountStorage.TryInsert(student).Result);
                }
            }

            logger.LogInformation("Student accounts are created.");

            var niceCourse = new Course(courseOwnerUsername)
            {
                Title = "Курс добра и позитива",
                Description = "В современном мире важно быть добрым и позитивным." +
                              " Данный курс предполагает не только знакомство с теорией" +
                              " добра и позитива, но и проверку полученных знаний на практике.",
                HasRoleTestResults = false,
                HasTeams = true,
                IsTeamStateLocked = false,
            };
            AssertIsCompleted(courseStorage.TryInsert(niceCourse).Result);
            var niceAnnouncement = new Announcement(niceCourse.Id)
            {
                Title = "Требования по курсу",
                Content = "На курсе обязательно нужно быть добрым и позитивным." +
                          "\nНельзя быть злым или негативным." +
                          "\n\nБольшое спасибо.",
            };
            AssertIsCompleted(announcementStorage.TryInsert(niceAnnouncement).Result);
            var niceHomework = new Homework(niceCourse.Id, isTeamwork: false)
            {
                Title = "Перестать быть злым и негативным (при необходимости)",
                Deadline = DateTime.UtcNow,
            };
            var kindHomework = new Homework(niceCourse.Id, isTeamwork: false)
            {
                Title = "Быть добрым",
                Content = "Требуется быть добрым до конца года. В качетсве подтверждения" +
                          " нужно будет привести отчет со всеми делами за год" +
                          " и провести оценку дел по признаку доброты в формате эссе.",
                Deadline = yearEnd,
            };
            var positiveHomework = new Homework(niceCourse.Id, isTeamwork: false)
            {
                Title = "Быть позитивным",
                Deadline = yearEnd,
            };
            var niceTeamHomework = new Homework(niceCourse.Id, isTeamwork: true)
            {
                Title = "Быть добрым и позитивным всей командой",
                Deadline = yearEnd,
            };
            AssertIsCompleted(homeworkStorage.TryInsert(niceHomework).Result);
            AssertIsCompleted(homeworkStorage.TryInsert(kindHomework).Result);
            AssertIsCompleted(homeworkStorage.TryInsert(positiveHomework).Result);
            AssertIsCompleted(homeworkStorage.TryInsert(niceTeamHomework).Result);
            var niceTeam = new Team(niceCourse.Id)
            {
                Name = "Команда добра и позитива",
            };
            AssertIsCompleted(teamStorage.TryInsert(niceTeam).Result);

            logger.LogInformation("Nice course is created.");

            var chineseCourse = new Course(courseOwnerUsername)
            {
                Title = "Факультатив по китайскому",
                Description = "Очевидно, что через 3 года основным международным языком" +
                              " станет китайский. После прохождения факультатива вы больше" +
                              " не будете переживать по этому поводу.",
                HasRoleTestResults = false,
                HasTeams = false,
                IsTeamStateLocked = false,
            };
            AssertIsCompleted(courseStorage.TryInsert(chineseCourse).Result);
            var chineseAnnouncement = new Announcement(chineseCourse.Id)
            {
                Title = "О формате проведения курса",
                Content = "Все лекции будут проходить исключительно на китайском языке.",
            };
            AssertIsCompleted(announcementStorage.TryInsert(chineseAnnouncement).Result);
            var russianHomework = new Homework(chineseCourse.Id, isTeamwork: false)
            {
                Title = "Забыть русский",
                Deadline = DateTime.UtcNow,
            };
            var englishHomework = new Homework(chineseCourse.Id, isTeamwork: false)
            {
                Title = "Забыть английский (при необходимости)",
                Content = "Задание нужно выполнять только тем, кто знает английский язык.",
                Deadline = DateTime.UtcNow,
            };
            var chineseHomework = new Homework(chineseCourse.Id, isTeamwork: false)
            {
                Title = "Выучить севернокитайский (мандаринский китайский) за 10 дней",
                Content = "Требуется сдать эссе на севернокитайском в размере" +
                          " не менее 2000 иероглифов." +
                          "\nЗащита дз пройдет в виде устного собеседования, на котором" +
                          " нужно будет аргументировать основные тезисы эссе.",
                ExtraUrl = "https://ru.wikipedia.org/wiki/%D0%9A%D0%B8%D1%82%D0%B0%D0%B9",
                ExtraUrlLabel = "Статья о том что такое Китай на Wiki",
                Deadline = DateTime.UtcNow.AddDays(10),
            };
            AssertIsCompleted(homeworkStorage.TryInsert(russianHomework).Result);
            AssertIsCompleted(homeworkStorage.TryInsert(englishHomework).Result);
            AssertIsCompleted(homeworkStorage.TryInsert(chineseHomework).Result);

            logger.LogInformation("Chinese course is created.");

            var effectiveCourse = new Course(courseOwnerUsername)
            {
                Title = "Эффективно-командный курс",
                Description = "Курс обязателен для тех, кто хочет быть эффективным и/или командным.",
                HasRoleTestResults = true,
                HasTeams = true,
                IsTeamStateLocked = false,
            };
            AssertIsCompleted(courseStorage.TryInsert(effectiveCourse).Result);
            var effectiveHomework = new Homework(effectiveCourse.Id, isTeamwork: true)
            {
                Title = "Быть эффективным и командным на протяжении месяца",
                Deadline = DateTime.UtcNow.AddMonths(1),
            };
            AssertIsCompleted(homeworkStorage.TryInsert(effectiveHomework).Result);
            var effectiveTeamIds = new int[3 + 1];
            for (int teamIndex = 1; teamIndex <= 3; teamIndex++)
            {
                var effectiveTeam = new Team(effectiveCourse.Id)
                {
                    Name = $"Команда эффективности {teamIndex}",
                };
                AssertIsCompleted(teamStorage.TryInsert(effectiveTeam).Result);
                effectiveTeamIds[teamIndex] = effectiveTeam.Id;
            }

            logger.LogInformation("Effective course is created.");

            var accounts = accountStorage.Select(_ => true).Result;
            var teachers = accounts.Where(account => account.Username.StartsWith("teacher"));
            var assistants = accounts.Where(account => account.Username.StartsWith("assistant"));
            var students = accounts.Where(
                    account => account.Role == UserRole.Student && !assistants.Contains(account)
            );

            int studentWithRoleResultCount = 0;
            foreach (var student in students)
            {
                if (studentWithRoleResultCount >= 15)
                {
                    break;
                }
                var testResult = new RoleTestResult(student.Username);
                for (int delta = 2; delta <= 3; delta++)
                {
                    var iterationCount = 70 / (2 + 3);
                    for (int index = 0; index < iterationCount; index++)
                    {
                        var secondRole = GetRandomRole();
                        testResult.SetScore(secondRole, testResult.GetScore(secondRole) + delta);
                    }
                }
                AssertIsCompleted(testResultStorage.TryInsert(testResult).Result);
                studentWithRoleResultCount++;
            }

            logger.LogInformation("Role test results course are created.");

            foreach (var teacher in teachers)
            {
                var niceMember = new CourseMember(
                    niceCourse.Id, teacher.Username, CourseRole.Teacher
                );
                var chineseMember = new CourseMember(
                    chineseCourse.Id, teacher.Username, CourseRole.Teacher
                );
                var effectiveMember = new CourseMember(
                    effectiveCourse.Id, teacher.Username, CourseRole.Teacher
                );
                AssertIsCompleted(memberStorage.TryInsert(niceMember).Result);
                AssertIsCompleted(memberStorage.TryInsert(chineseMember).Result);
                AssertIsCompleted(memberStorage.TryInsert(effectiveMember).Result);
            }
            foreach (var assistant in assistants)
            {
                var niceMember = new CourseMember(
                    niceCourse.Id, assistant.Username, CourseRole.Assistant
                );
                var chineseMember = new CourseMember(
                    chineseCourse.Id, assistant.Username, CourseRole.Assistant
                );
                var effectiveMember = new CourseMember(
                    effectiveCourse.Id, assistant.Username, CourseRole.Assistant
                );
                AssertIsCompleted(memberStorage.TryInsert(niceMember).Result);
                AssertIsCompleted(memberStorage.TryInsert(chineseMember).Result);
                AssertIsCompleted(memberStorage.TryInsert(effectiveMember).Result);
            }

            logger.LogInformation("Teacher and assistant members are created.");

            int studentFromNiceTeamCount = 0;
            int studentFromEffectiveTeamCount = 0;
            foreach (var student in students)
            {
                var niceMember = new CourseMember(
                    niceCourse.Id, student.Username, CourseRole.Student
                );
                var chineseMember = new CourseMember(
                    chineseCourse.Id, student.Username, CourseRole.Student
                );
                var effectiveMember = new CourseMember(
                    effectiveCourse.Id, student.Username, CourseRole.Student
                );

                if (studentFromNiceTeamCount < 10)
                {
                    niceMember.TeamId = niceTeam.Id;
                    studentFromNiceTeamCount++;
                }
                if (studentFromEffectiveTeamCount < 15)
                {
                    var effectiveTeamIndex = studentFromEffectiveTeamCount % 3 + 1;
                    effectiveMember.TeamId = effectiveTeamIds[effectiveTeamIndex];
                    studentFromEffectiveTeamCount++;
                }

                AssertIsCompleted(memberStorage.TryInsert(niceMember).Result);
                AssertIsCompleted(memberStorage.TryInsert(chineseMember).Result);
                AssertIsCompleted(memberStorage.TryInsert(effectiveMember).Result);

                var niceHomeworkStatus = new HomeworkStatus(niceHomework.Id, student.Username);
                niceHomeworkStatus.MarkAsSumbitted(student.Username);
                niceHomeworkStatus.Evaluate(10, teachers.First().Username);
                var russianHomeworkStatus = new HomeworkStatus(russianHomework.Id, student.Username);
                russianHomeworkStatus.MarkAsSumbitted(student.Username);
                russianHomeworkStatus.Evaluate(0, assistants.First().Username);
                AssertIsCompleted(homeworkStatusStorage.TryInsert(niceHomeworkStatus).Result);
                AssertIsCompleted(homeworkStatusStorage.TryInsert(russianHomeworkStatus).Result);
            }

            logger.LogInformation("Student members are created.");
            logger.LogInformation("Content generation is done.");
        }

        private void AssertIsCompleted(bool actionResult)
        {
            if (!actionResult)
            {
                logger.LogError("Storage operation is not completed.");
                throw new NotSupportedException("Storage operation is not completed.");
            }
        }

        private TeamRole GetRandomRole()
        {
            var values = Enum.GetValues<TeamRole>().ToList();
            return values[randomGenerator.Next(values.Count)];
        }

        private static void TruncateStorage<TKey, TValue>(IStorage<TKey, TValue> storage)
            where TValue : class
        {
            var allEntities = storage.Select(_ => true).Result;
            foreach (var entity in allEntities)
            {
                storage.TryDelete(entity).Wait();
            }
        }
    }
}