namespace HomemadeLMS.Models.Domain
{
    public enum CourseRole
    {
        Student,
        Assistant,
        Teacher,
    }

    public class CourseMember
    {
        private string username;

        public CourseMember(int courseId, string username, CourseRole role)
        {
            CourseId = courseId;
            Username = username;
            Role = role;
            this.username = Username;
        }

        public int RecordId { get; set; }
        public int CourseId { get; set; }
        public CourseRole Role { get; set; }

        public string Username
        {
            get => username;
            set
            {
                if (!Account.HasUsernameValidFormat(value))
                {
                    throw new ArgumentException("Invalid username format.");
                }
                username = value;
            }
        }
    }
}
