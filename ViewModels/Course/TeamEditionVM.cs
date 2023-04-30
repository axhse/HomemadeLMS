using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public class TeamEditionVM
    {
        public TeamEditionVM(Team team)
        {
            Team = team;
        }

        public Team Team { get; private set; }
        public List<CourseMember> Members { get; set; } = new();
    }
}