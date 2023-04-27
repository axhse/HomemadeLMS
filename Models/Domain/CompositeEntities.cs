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
}