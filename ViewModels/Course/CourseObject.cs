using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public class CourseObject<TObject>
    {
        public CourseObject(Account account, Course course, TObject obj)
        {
            Account = account;
            Course = course;
            Object = obj;
        }

        public Account Account { get; private set; }
        public Course Course { get; private set; }
        public TObject Object { get; private set; }
    }
}