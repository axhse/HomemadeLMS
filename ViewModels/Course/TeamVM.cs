using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public class TeamVM
    {
        public TeamVM(Course course, CourseMember courseMember, Team team)
        {
            Course = course;
            CourseMember = courseMember;
            Team = team;
        }

        public Course Course { get; private set; }
        public CourseMember CourseMember { get; private set; }
        public Team Team { get; private set; }
        public List<CourseMemberAndAccount> Members { get; set; } = new();
    }
}