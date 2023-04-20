namespace HomemadeLMS.Models.Domain
{
    public class Course
    {
        public const int MaxTitleSize = 200;
        public const int MaxDescriptionSize = 2500;
        public const int MaxUrlSize = 1000;

        private const string DefaulCourseTitle = "New Course";

        private string ownerUsername;
        private string title;
        private string? description;
        private string? smartLmsUrl;
        private string? pldUrl;

        public Course(string ownerUsername)
        {
            OwnerUsername = ownerUsername;
            title = DefaulCourseTitle;
            this.ownerUsername = OwnerUsername;
        }

        public static bool HasTitleValidFormat(string? title)
            => title is not null && 1 <= title.Length && title.Length <= MaxTitleSize;

        public static bool HasDescriptionValidFormat(string? description)
            => description is null || description.Length <= MaxDescriptionSize;

        public static bool HasUrlValidFormat(string? url)
            => url is null || url.Length <= MaxUrlSize;

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
                if (!HasTitleValidFormat(value))
                {
                    throw new ArgumentException("Invalid title format.");
                }
                title = value;
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

        public string? SmartLmsUrl
        {
            get => smartLmsUrl;
            set
            {
                if (!HasUrlValidFormat(value))
                {
                    throw new ArgumentException("Invalid smartLmsUrl format.");
                }
                smartLmsUrl = value;
            }
        }

        public string? PldUrl
        {
            get => pldUrl;
            set
            {
                if (!HasUrlValidFormat(value))
                {
                    throw new ArgumentException("Invalid pldUrl format.");
                }
                pldUrl = value;
            }
        }

        public bool CanBeEditedBy(Account account)
            => account.Role == UserRole.Manager || OwnerUsername == account.Username;
    }
}