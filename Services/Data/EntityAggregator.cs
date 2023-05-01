using HomemadeLMS.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace HomemadeLMS.Services.Data
{
    public class EntityAggregator
    {
        private readonly CompositeContext context;

        public EntityAggregator(CompositeContext context)
        {
            this.context = context;
        }

        public async Task<List<PersonalHomework>> GetAllPersonalHomework(
            int courseId, string subjectId)
        {
            var query = from homeworkStatus in context.AllHomeworkStatus
                        where homeworkStatus.SubjectId == subjectId
                        join homework in context.AllHomework
                        on homeworkStatus.HomeworkId equals homework.Id
                        where homework.CourseId == courseId
                        select new PersonalHomework(homework, homeworkStatus);
            return await query.ToListAsync();
        }

        public async Task<List<MemberInfo>> GetCourseMemberInfo(int courseId)
        {
            var query = from member in context.Members
                        where member.CourseId == courseId
                        join account in context.Accounts
                        on member.Username equals account.Username
                        select new MemberInfo(member, account);
            return await query.ToListAsync();
        }

        public async Task<List<Course>> GetUserCourses(string username)
        {
            var query = from member in context.Members
                        where member.Username == username
                        join course in context.Courses
                        on member.CourseId equals course.Id
                        select course;
            return await query.ToListAsync();
        }
    }
}