using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public class TeamsVM
    {
        public TeamsVM(Course course, CourseMember courseMember)
        {
            Course = course;
            CourseMember = courseMember;
        }

        public Course Course { get; private set; }
        public CourseMember CourseMember { get; private set; }
        public List<CourseMember> MembersWithoutTeam { get; set; } = new();
        public List<Team> Teams { get; set; } = new();
    }
}