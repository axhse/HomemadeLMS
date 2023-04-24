using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.Services.Data
{
    public class AccountContext : GenericContext<Account>
    {
        public AccountContext() : base(new AccountConfiguration())
        { }
    }

    public class CourseContext : GenericContext<Course>
    {
        public CourseContext() : base(new CourseConfiguration())
        { }
    }

    public class CourseMemberContext : GenericContext<CourseMember>
    {
        public CourseMemberContext() : base(new CourseMemberConfiguration())
        { }
    }

    public class RoleTestResultContext : GenericContext<RoleTestResult>
    {
        public RoleTestResultContext() : base(new RoleTestResultConfiguration())
        { }
    }

    public class AnnouncementContext : GenericContext<Announcement>
    {
        public AnnouncementContext() : base(new AnnouncementConfiguration())
        { }
    }
}