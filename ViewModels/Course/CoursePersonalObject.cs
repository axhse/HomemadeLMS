using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public class CoursePersonalObject<TObject>
    {
        public CoursePersonalObject(Course course, CourseMember courseMember, TObject obj)
        {
            Course = course;
            CourseMember = courseMember;
            Object = obj;
        }

        public Course Course { get; private set; }
        public CourseMember CourseMember { get; private set; }
        public TObject Object { get; private set; }
    }
}