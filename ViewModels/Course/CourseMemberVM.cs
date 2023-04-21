using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public class CourseMemberVM
    {
        public CourseMemberVM(Account account, Course course, CourseMember member)
        {
            Account = account;
            Course = course;
            Member = member;
        }

        public Account Account { get; private set; }
        public Course Course { get; private set; }
        public CourseMember Member { get; private set; }
    }
}