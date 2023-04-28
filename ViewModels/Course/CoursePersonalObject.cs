using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public class CourseMemberAndObject<TObject>
    {
        public CourseMemberAndObject(CourseMember courseMember, TObject obj)
        {
            CourseMember = courseMember;
            Object = obj;
        }

        public CourseMember CourseMember { get; private set; }
        public TObject Object { get; private set; }

        public int CourseId => CourseMember.CourseId;
    }
}