using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public static class Comparators
    {
        private const int IsLess = -1;
        private const int IsMore = 1;

        public static Comparison<MemberInfo> MemberInfo
            => (first, second) => CompareMemberInfo(first, second);

        public static Comparison<MemberInfo> TeamMemberInfo(Team team)
            => (first, second) => CompareTeamMemberInfo(first, second, team);

        public static Comparison<CourseMember> TeamMembers(Team team)
            => (first, second) => CompareTeamMembers(first, second, team);

        private static int CompareMemberInfo(MemberInfo first, MemberInfo second)
        {
            var firstName = first.Account?.Name;
            var secondName = second.Account?.Username;
            if (firstName is null)
            {
                if (secondName is null)
                {
                    return first.Member.Username.CompareTo(second.Member.Username);
                }
                else
                {
                    return IsMore;
                }
            }
            if (secondName is null)
            {
                return IsLess;
            }
            if (firstName.CompareTo(secondName) != 0)
            {
                return firstName.CompareTo(secondName);
            }
            return first.Member.Username.CompareTo(second.Member.Username);
        }

        private static int CompareTeamMemberInfo(MemberInfo first, MemberInfo second, Team team)
        {
            if (first.Member.IsLeader(team) == second.Member.IsLeader(team))
            {
                return CompareMemberInfo(first, second);
            }
            return first.Member.IsLeader(team) ? IsLess : IsMore;
        }

        private static int CompareTeamMembers(CourseMember first, CourseMember second, Team team)
            => CompareTeamMemberInfo(new(first), new(second), team);
    }
}