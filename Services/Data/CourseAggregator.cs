using HomemadeLMS.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace HomemadeLMS.Services.Data
{
    public class CourseAggregator
    {
        private readonly CourseCompositeContext context;

        public CourseAggregator(CourseCompositeContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<CourseInfo>> GetUserCourses(string username)
        {
            var query = from courseMember in context.CourseMembers
                        where courseMember.Username == username
                        join course in context.Courses
                        on courseMember.CourseId equals course.Id
                        select new CourseInfo
                        {
                            Id = course.Id,
                            Title = course.Title,
                        };
            return await query.ToListAsync();
        }
    }
}