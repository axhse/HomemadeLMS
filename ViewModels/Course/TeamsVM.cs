using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public class TeamsVM
    {
        public TeamsVM(Course course, CourseMember member)
        {
            Course = course;
            Member = member;
        }

        public TeamsVM(Course course, CourseMember member,
            List<Team> teams, List<CourseMember> singleStudents) : this(course, member)
        {
            Teams = teams;
            SingleStudents = singleStudents;
        }

        public Course Course { get; private set; }
        public CourseMember Member { get; private set; }
        public List<CourseMember> SingleStudents { get; set; } = new();
        public List<Team> Teams { get; set; } = new();
    }
}