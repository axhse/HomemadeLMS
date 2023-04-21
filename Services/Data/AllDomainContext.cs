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
}