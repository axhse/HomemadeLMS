using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public class MemberAndObject<TObject>
    {
        public MemberAndObject(CourseMember member, TObject obj)
        {
            Member = member;
            Object = obj;
        }

        public CourseMember Member { get; private set; }
        public TObject Object { get; private set; }

        public int CourseId => Member.CourseId;
    }
}