namespace HomemadeLMS.Models.Domain
{
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
}