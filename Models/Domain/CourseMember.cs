﻿namespace HomemadeLMS.Models.Domain
{
    public enum CourseRole
    {
        Assistant,
        Spectator,
        Student,
        Teacher,
    }

    public class CourseMember
    {
        public const string UidSeparator = ":";
        public static readonly int MaxUidSize = DataUtils.MaxNumericStringSize
                                                + UidSeparator.Length + Account.MaxUsernameSize;
        private string username;

        public CourseMember(int courseId, string username, CourseRole role)
        {
            CourseId = courseId;
            Username = username;
            Role = role;
            this.username = Username;
        }

        public static string BuildUid(int courseId, string username)
            => $"{courseId}{UidSeparator}{username}";

        public static CourseMember BuildSpectator(int courseId, string username)
            => new(courseId, username, CourseRole.Spectator);

        public int CourseId { get; set; }
        public int? TeamId { get; set; }
        public CourseRole Role { get; set; }

        public string Uid
        {
            get => BuildUid(CourseId, Username);
            set { }
        }

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

        public bool IsStudent => Role == CourseRole.Student;
        public bool IsTeacher => Role == CourseRole.Teacher;

        public bool CanChangeTeam(Course course) => IsStudent
            && course.HasTeams && !course.IsTeamStateLocked;

        public bool CanCreateTeam(Course course) => !IsStudent || !course.IsTeamStateLocked;

        public bool CanEditTeam(Course course, Team team) => !IsStudent
            || (team.LeaderUsername == Username && !course.IsTeamStateLocked);
    }
}