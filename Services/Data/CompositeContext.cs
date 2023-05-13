using HomemadeLMS.Environment;
using HomemadeLMS.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace HomemadeLMS.Services.Data
{
    public class CompositeContext : DbContext
    {
        public CompositeContext() : base()
        { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseMember> Members { get; set; }
        public DbSet<Homework> AllHomework { get; set; }
        public DbSet<HomeworkStatus> AllHomeworkStatus { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = Program.SecretManager.Get(SecretName.DatabaseConnectionString);
            optionsBuilder.UseSqlServer(connectionString);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AccountConfiguration());
            modelBuilder.ApplyConfiguration(new CourseConfiguration());
            modelBuilder.ApplyConfiguration(new CourseMemberConfiguration());
            modelBuilder.ApplyConfiguration(new HomeworkConfiguration());
            modelBuilder.ApplyConfiguration(new HomeworkStatusConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}