using Microsoft.EntityFrameworkCore;

namespace HomemadeLMS.Services.Data
{
    public class GenericContext<TEntity> : DbContext where TEntity : class
    {
        private readonly IEntityTypeConfiguration<TEntity> entityConfiguration;

        protected GenericContext(IEntityTypeConfiguration<TEntity> entityConfiguration) : base()
        {
            this.entityConfiguration = entityConfiguration;
        }

        public DbSet<TEntity> Items { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Program.AppConfig.DatabaseConfig.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(entityConfiguration);
            base.OnModelCreating(modelBuilder);
        }
    }
}