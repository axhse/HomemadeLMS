using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public class TeamVM
    {
        public TeamVM(Course course, CourseMember member, Team team)
        {
            Course = course;
            Member = member;
            Team = team;
        }

        public Course Course { get; private set; }
        public CourseMember Member { get; private set; }
        public Team Team { get; private set; }
        public List<MemberInfo> AllMemberInfo { get; set; } = new();
    }
}