using HomemadeLMS.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomemadeLMS.Services.Data
{
    public class TeamConfiguration : IEntityTypeConfiguration<Team>
    {
        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.HasKey(team => team.Id);
            builder.Property(team => team.Id).ValueGeneratedOnAdd();
            builder.Property(team => team.CourseId).IsRequired();
            builder.Property(team => team.LeaderUsername).HasMaxLength(Account.MaxUsernameSize);
            builder.Property(team => team.Name).IsRequired();
            builder.Property(team => team.Name).HasMaxLength(2 * Team.MaxNameSize);
            builder.Property(team => team.Name).HasConversion<StringHexConverter>();
            builder.ToTable(nameof(Team));
        }
    }
}