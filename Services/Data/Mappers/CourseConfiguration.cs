using HomemadeLMS.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomemadeLMS.Services.Data
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.HasKey(course => course.Id);
            builder.Property(course => course.Id).ValueGeneratedOnAdd();
            builder.Property(course => course.Title).IsRequired();
            builder.Property(course => course.Title).HasMaxLength(2 * Course.MaxTitleSize);
            builder.Property(course => course.Title).HasConversion<StringHexConverter>();
            builder.Property(course => course.OwnerUsername).HasMaxLength(Account.MaxUsernameSize);
            builder.Property(course => course.Description).HasMaxLength(2 * Course.MaxDescriptionSize);
            builder.Property(course => course.Description).HasConversion<StringHexConverter>();
            builder.Property(course => course.SmartLmsUrl).HasMaxLength(2 * Course.MaxUrlSize);
            builder.Property(course => course.SmartLmsUrl).HasConversion<StringHexConverter>();
            builder.Property(course => course.PldUrl).HasMaxLength(2 * Course.MaxUrlSize);
            builder.Property(course => course.PldUrl).HasConversion<StringHexConverter>();
            builder.Property(course => course.HasTeams).IsRequired();
            builder.Property(course => course.IsTeamStateLocked).IsRequired();
            builder.ToTable(nameof(Course));
        }
    }
}