using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public class MemberAndAllPersonalHomework: MemberAndObject<List<PersonalHomework>>
    {
        public MemberAndAllPersonalHomework(CourseMember member, List<PersonalHomework> allHomework,
                                            Course course)  : base(member, allHomework)
        {
            HasTeams = course.HasTeams;
        }

        public bool HasTeams { get; private set; }
    }
}