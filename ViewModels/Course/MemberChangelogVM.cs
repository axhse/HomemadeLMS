using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public class MemberChangelogVM
    {
        public MemberChangelogVM(Course course)
        {
            Course = course;
        }

        public Course Course { get; private set; }
        public List<string> AddedUsernames { get; private set; } = new();
        public List<string> AlreadyAddedUsernames { get; private set; } = new();
        public List<string> RemovedUsernames { get; private set; } = new();
        public List<string> AlreadyRemovedUsernames { get; private set; } = new();
        public List<string> InvalidUsernames { get; private set; } = new();
    }
}