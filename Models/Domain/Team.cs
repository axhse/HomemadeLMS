namespace HomemadeLMS.Models.Domain
{
	public class Team
    {
        public const int MaxNameSize = 100;
        public const string TagStarting = "#t";
		public static readonly int TagSize = TagStarting.Length + DataUtils.MaxNumericStringSize;

        private const string DefaultName = "New Team";
        private string name;
        private string? leaderUsername;

        public Team(int courseId)
        {
            name = DefaultName;
            CourseId = courseId;
        }

        public static bool HasNameValidFormat(string? name)
        {
            name = DataUtils.CleanSpaces(name);
            return name is not null && name != string.Empty && name.Length <= MaxNameSize;
        }

        public static bool HasTagValidFormat(string? tag)
		{
			return tag is not null
				   && tag.StartsWith(TagStarting)
				   && int.TryParse(tag[TagStarting.Length..], out var _);
        }

		public static bool TryGetId(string? tag, out int id)
		{
			id = 0;
			if (tag is null || !tag.StartsWith(TagStarting))
			{
				return false;
			}
			return int.TryParse(tag[TagStarting.Length..], out id);
        }

        public static string BuildTag(int teamId) => $"{TagStarting}{teamId}";

        public int CourseId { get; set; }
        public int Id { get; set; }

        public string Name
        {
            get => name;
            set
            {
                var newName = DataUtils.CleanSpaces(value);
                if (newName is null || !HasNameValidFormat(newName))
                {
                    throw new ArgumentException("Invalid name format.");
                }
                name = newName;
            }
        }

        public string? LeaderUsername
        {
            get => leaderUsername;
            set
            {
                if (value is not null && !Account.HasUsernameValidFormat(value))
                {
                    throw new ArgumentException("Invalid username format.");
                }
                leaderUsername = value;
            }
        }

        public string Tag => BuildTag(Id);
    }
}