using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public class MemberRoles
    {
        public MemberRoles(MemberInfo memberInfo, RoleTestResult? testResult = null)
        {
            MemberInfo = memberInfo;
            TestResult = testResult;
        }

        public MemberInfo MemberInfo { get; private set; }
        public RoleTestResult? TestResult { get; private set; }
    }

    public class TeamRoles
    {
        public TeamRoles(Team team)
        {
            TeamId = team.Id;
        }

        public TeamRoles(Team team, List<MemberRoles> memberRoles) : this(team)
        {
            AllMemberRoles = memberRoles;
        }

        public List<MemberRoles> AllMemberRoles { get; set; } = new();
        public int TeamId { get; private set; }
    }
}