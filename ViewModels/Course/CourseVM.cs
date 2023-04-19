using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public class CourseVM
    {
        public CourseVM(Account account, Course course)
        {
            Account = account;
            Course = course;
        }

        public Account Account { get; private set; }
        public Course Course { get; private set; }
    }
}