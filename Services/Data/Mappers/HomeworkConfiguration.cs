using HomemadeLMS.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomemadeLMS.Services.Data
{
    public class HomeworkConfiguration : IEntityTypeConfiguration<Homework>
    {
        public void Configure(EntityTypeBuilder<Homework> builder)
        {
            builder.HasKey(homework => homework.Id);
            builder.Property(homework => homework.Id).ValueGeneratedOnAdd();
            builder.Property(homework => homework.CourseId).IsRequired();
            builder.Property(homework => homework.CreationTime).IsRequired();
            builder.Property(homework => homework.IsTeamwork).IsRequired();
            builder.Property(homework => homework.Title).IsRequired();
            builder.Property(homework => homework.Title)
                   .HasMaxLength(2 * Announcement.MaxTitleSize);
            builder.Property(homework => homework.Title).HasConversion<StringHexConverter>();
            builder.Property(homework => homework.Content)
                   .HasMaxLength(2 * Announcement.MaxContentSize);
            builder.Property(homework => homework.Content).HasConversion<StringHexConverter>();
            builder.Property(homework => homework.TaskUrl).HasMaxLength(2 * Course.MaxUrlSize);
            builder.Property(homework => homework.TaskUrl).HasConversion<StringHexConverter>();
            builder.Property(homework => homework.SubmitUrl).HasMaxLength(2 * Course.MaxUrlSize);
            builder.Property(homework => homework.SubmitUrl).HasConversion<StringHexConverter>();
            builder.Property(homework => homework.ExtraUrl).HasMaxLength(2 * Course.MaxUrlSize);
            builder.Property(homework => homework.ExtraUrl).HasConversion<StringHexConverter>();
            builder.Property(homework => homework.ExtraUrlLabel)
                   .HasMaxLength(2 * Homework.MaxUrlLabelSize);
            builder.Property(homework => homework.ExtraUrlLabel).HasConversion<StringHexConverter>();
            builder.ToTable(nameof(Homework));
        }
    }
}