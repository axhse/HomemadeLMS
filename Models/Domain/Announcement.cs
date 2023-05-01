namespace HomemadeLMS.Models.Domain
{
	public class Announcement
    {
        public const int MaxContentSize = 3000;
        public const int MaxTitleSize = 500;

        private const string DefaultTitle = "New Announcement";
        private string title;
        private string? content;

        public Announcement(int courseId)
        {
            title = DefaultTitle;
            CourseId = courseId;
            CreationTime = DateTime.UtcNow;
        }

        public static bool HasContentValidFormat(string? content)
            => content is null || content.Length <= MaxContentSize;

        public static bool HasTitleValidFormat(string? title)
        {
            title = DataUtils.CleanSpaces(title);
            return title is not null && title != string.Empty && title.Length <= MaxTitleSize;
        }

        public int CourseId { get; set; }
        public int Id { get; set; }
        public DateTime CreationTime { get; set; }

        public string Title
        {
            get => title;
            set
            {
                var newTitle = DataUtils.CleanSpaces(value);
                if (newTitle is null || !HasTitleValidFormat(newTitle))
                {
                    throw new ArgumentException("Invalid title format.");
                }
                title = newTitle;
            }
        }

        public string? Content
        {
            get => content;
            set
            {
                if (!HasContentValidFormat(value))
                {
                    throw new ArgumentException("Invalid content format.");
                }
                content = value;
            }
        }
    }
}