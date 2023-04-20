using HomemadeLMS.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace HomemadeLMS.Services.Data
{
    public class CourseCompositeContext : DbContext
    {
        public CourseCompositeContext() : base()
        { }

        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseMember> CourseMembers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Program.AppConfig.DatabaseConfig.DbConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CourseConfiguration());
            modelBuilder.ApplyConfiguration(new CourseMemberConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}