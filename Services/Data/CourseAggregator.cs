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

        public async Task<List<Course>> GetUserCourses(string username)
        {
            var query = from courseMember in context.CourseMembers
                        where courseMember.Username == username
                        join course in context.Courses
                        on courseMember.CourseId equals course.Id
                        select course;
            return await query.ToListAsync();
        }
    }
}