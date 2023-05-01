namespace HomemadeLMS.Models.Domain
{
    public class CourseMemberAndAccount
    {
        public CourseMemberAndAccount(CourseMember courseMember, Account? account = null)
        {
            CourseMember = courseMember;
            Account = account;
        }

        public Account? Account { get; private set; }
        public CourseMember CourseMember { get; private set; }
    }

    public class HomeworkWithAllStatus
    {
        public HomeworkWithAllStatus(Homework homework)
        {
            Homework = homework;
        }
        public HomeworkWithAllStatus(Homework homework, List<HomeworkStatus> allStatus)
        {
            Homework = homework;
            AllStatus = allStatus;
        }

        public Homework Homework { get; private set; }
        public List<HomeworkStatus> AllStatus { get; set; } = new();
    }

    public class PersonalHomework
    {
        public PersonalHomework(Homework homework, HomeworkStatus homeworkStatus)
        {
            Homework = homework;
            HomeworkStatus = homeworkStatus;
        }

        public Homework Homework { get; private set; }
        public HomeworkStatus HomeworkStatus { get; private set; }
    }
}