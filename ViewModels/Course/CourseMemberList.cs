using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
	public class CourseMembers
	{
		public CourseMembers(Account account, Course course, IEnumerable<CourseMember> members)
		{
			Account = account;
            Course = course;
            Members = members;
		}

		public Account Account { get; private set; }
		public Course Course { get; private set; }
		public IEnumerable<CourseMember> Members { get; private set; }
    }
}