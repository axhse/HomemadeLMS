namespace HomemadeLMS.Models.Domain
{
	public class Team
	{
		public const string TagStarting = "#t";
		public static readonly int TagSize = TagStarting.Length + DataUtils.MaxNumericStringSize;

        public static bool HasTagValidFormat(string? tag)
		{
			return tag is not null
				   && tag.StartsWith(TagStarting)
				   && int.TryParse(tag[TagStarting.Length..], out var _);
        }

		public static string BuildTag(int teamId) => $"{TagStarting}{teamId}";
		public static bool TryGetId(string? tag, out int id) => int.TryParse(tag, out id);

        public int Id { get; set; }
		public string Tag => BuildTag(Id);
	}
}