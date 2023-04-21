namespace HomemadeLMS.Models
{
	public static class DataUtils
	{
		public static string? GetTrimmed(string? text)
		{
			if (text is null)
			{
				return null;
			}
			return text.Trim();
		}

		public static bool IsValuable(string? text)
		{
			return text is not null && text.Trim() != string.Empty;
		}
	}
}
