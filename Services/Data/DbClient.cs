using Microsoft.EntityFrameworkCore;

namespace HomemadeLMS.Services.Data
{
    public class DbClient<TEntity> : DbContext where TEntity : class
    {
        private readonly IEntityTypeConfiguration<TEntity> entityConfiguration;

        protected DbClient(IEntityTypeConfiguration<TEntity> entityConfiguration)
            : base(new DbContextOptions<DbClient<TEntity>>())
        {
            this.entityConfiguration = entityConfiguration;
            Database.SetConnectionString(Program.AppConfig.DatabaseConfig.DbConnectionString);
            SaveChangesAsync();    // TODO: remove?
        }

        public DbSet<TEntity> Items { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer();
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(entityConfiguration);
            base.OnModelCreating(modelBuilder);
        }
    }
}