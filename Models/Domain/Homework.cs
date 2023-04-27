namespace HomemadeLMS.Models.Domain
{
	public class Homework
    {
        public const int MaxUrlLabelSize = 200;
        private const string DefaultTitle = "New Task";

        private string title;
        private string? content;
        private string? taskUrl;
        private string? submitUrl;
        private string? extraUrl;
        private string? extraUrlLabel;

        public Homework(int courseId, bool isTeamwork)
        {
            title = DefaultTitle;
            CourseId = courseId;
            CreationTime = DateTime.UtcNow;
            IsTeamwork = isTeamwork;
        }

        public static bool HasUrlLabelValidFormat(string? urlLabel)
        {
            urlLabel = DataUtils.CleanSpaces(urlLabel);
            return urlLabel is null || urlLabel.Length <= MaxUrlLabelSize;
        }

        public int Id { get; set; }
        public int CourseId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? DeadlineTime { get; set; }
        public bool IsTeamwork { get; init; }

        public string Title
        {
            get => title;
            set
            {
                var newTitle = DataUtils.CleanSpaces(value);
                if (newTitle is null || !Announcement.HasTitleValidFormat(newTitle))
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
                if (!Announcement.HasContentValidFormat(value))
                {
                    throw new ArgumentException("Invalid content format.");
                }
                content = value;
            }
        }

        public string? TaskUrl
        {
            get => taskUrl;
            set
            {
                var newUrl = DataUtils.GetTrimmed(value);
                if (!Course.HasUrlValidFormat(newUrl))
                {
                    throw new ArgumentException("Invalid taskUrl format.");
                }
                taskUrl = newUrl;
            }
        }

        public string? SubmitUrl
        {
            get => submitUrl;
            set
            {
                var newUrl = DataUtils.GetTrimmed(value);
                if (!Course.HasUrlValidFormat(newUrl))
                {
                    throw new ArgumentException("Invalid submitUrl format.");
                }
                submitUrl = newUrl;
            }
        }

        public string? ExtraUrl
        {
            get => extraUrl;
            set
            {
                var newUrl = DataUtils.GetTrimmed(value);
                if (!Course.HasUrlValidFormat(newUrl))
                {
                    throw new ArgumentException("Invalid extraUrl format.");
                }
                extraUrl = newUrl;
            }
        }

        public string? ExtraUrlLabel
        {
            get => extraUrlLabel;
            set
            {
                value = DataUtils.CleanSpaces(value);
                if (!HasUrlLabelValidFormat(value))
                {
                    throw new ArgumentException("Invalid extraUrlLabel format.");
                }
                extraUrlLabel = value;
            }
        }

        public bool IsDeadlineExpired => DeadlineTime is not null && DeadlineTime < DateTime.UtcNow;
    }
}