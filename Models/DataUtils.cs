using System.Text.RegularExpressions;

namespace HomemadeLMS.Models
{
	public static class DataUtils
	{
		public const char SpaceChar = '\u0020';

		public static string? GetTrimmed(string? text)
		{
			if (text is null)
			{
				return null;
			}
			return text.Trim();
        }

        public static string? CleanSpaces(string? text)
        {
            if (text is null)
            {
                return null;
            }
			text = text.Trim();
            text = Regex.Replace(text, "[\t\r\n\f]", string.Empty);
            text = Regex.Replace(text, $"{SpaceChar}{{2,}}", $"{SpaceChar}");
			return text;
        }

        public static bool IsValuable(string? text)
		{
			return text is not null && text.Trim() != string.Empty;
		}
	}
}
