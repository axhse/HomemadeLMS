namespace HomemadeLMS.Models.Domain
{
    public class HomeworkInfo
    {
        public HomeworkInfo(Homework homework)
        {
            Homework = homework;
        }

        public HomeworkInfo(Homework homework, List<HomeworkStatus> allStatus)
        {
            Homework = homework;
            AllStatus = allStatus;
        }

        public Homework Homework { get; private set; }
        public List<HomeworkStatus> AllStatus { get; set; } = new();
    }

    public class MemberInfo
    {
        public MemberInfo(CourseMember member, Account? account = null)
        {
            Member = member;
            Account = account;
        }

        public CourseMember Member { get; private set; }
        public Account? Account { get; private set; }
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