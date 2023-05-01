using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
	public static class Comparators
    {
        private const int IsLess = -1;
        private const int IsMore = 1;

		public static Comparison<MemberInfo> ByNameThenByUsername
			=> (first, second) => CompareByNameThenByUsername(first, second);

        private static int CompareByNameThenByUsername(MemberInfo first, MemberInfo second)
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
	}
}
