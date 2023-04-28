using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public class CourseAndObject<TObject>
    {
        public CourseAndObject(Account account, Course course, TObject obj)
        {
            Course = course;
            Object = obj;
            IsCourseEditable = course.CanBeEditedBy(account);
        }

        public Course Course { get; private set; }
        public TObject Object { get; private set; }

        public bool IsCourseEditable { get; private set; }
    }
}