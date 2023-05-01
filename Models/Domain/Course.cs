namespace HomemadeLMS.Models.Domain
{
    public class Course
    {
        public const int MaxDescriptionSize = 2500;
        public const int MaxTitleSize = 200;
        public const int MaxUrlSize = 1000;
        public const string DefaultTitle = "New Course";

        private string ownerUsername;
        private string title;
        private string? description;
        private string? pldUrl;
        private string? smartLmsUrl;

        public Course(string ownerUsername)
        {
            title = DefaultTitle;
            OwnerUsername = ownerUsername;
            this.ownerUsername = OwnerUsername;
        }

        public static bool HasDescriptionValidFormat(string? description)
            => description is null || description.Length <= MaxDescriptionSize;

        public static bool HasTitleValidFormat(string? title)
        {
            title = DataUtils.CleanSpaces(title);
            return title is not null && title != string.Empty && title.Length <= MaxTitleSize;
        }

        public static bool HasUrlValidFormat(string? url)
        {
            url = DataUtils.GetTrimmed(url);
            return url is null || url.Length <= MaxUrlSize;
        }

        public bool HasTeams { get; set; } = false;
        public bool IsTeamStateLocked { get; set; } = false;
        public int Id { get; set; }

        public string OwnerUsername
        {
            get => ownerUsername;
            set
            {
                if (!Account.HasUsernameValidFormat(value))
                {
                    throw new ArgumentException("Invalid username format.");
                }
                ownerUsername = value;
            }
        }

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

        public string? Description
        {
            get => description;
            set
            {
                if (!HasDescriptionValidFormat(value))
                {
                    throw new ArgumentException("Invalid description format.");
                }
                description = value;
            }
        }

        public string? PldUrl
        {
            get => pldUrl;
            set
            {
                var newUrl = DataUtils.GetTrimmed(value);
                if (!HasUrlValidFormat(newUrl))
                {
                    throw new ArgumentException("Invalid pldUrl format.");
                }
                pldUrl = newUrl;
            }
        }

        public string? SmartLmsUrl
        {
            get => smartLmsUrl;
            set
            {
                var newUrl = DataUtils.GetTrimmed(value);
                if (!HasUrlValidFormat(newUrl))
                {
                    throw new ArgumentException("Invalid smartLmsUrl format.");
                }
                smartLmsUrl = newUrl;
            }
        }

        public bool CanBeEditedBy(Account account) => account.Role == UserRole.Manager
            || (account.Role == UserRole.Teacher && OwnerUsername == account.Username);
    }
}