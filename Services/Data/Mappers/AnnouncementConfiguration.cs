using HomemadeLMS.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomemadeLMS.Services.Data
{
    public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
    {
        public void Configure(EntityTypeBuilder<Announcement> builder)
        {
            builder.HasKey(announcement => announcement.Id);
            builder.Property(announcement => announcement.Id).ValueGeneratedOnAdd();
            builder.Property(announcement => announcement.CourseId).IsRequired();
            builder.Property(announcement => announcement.CreationTime).IsRequired();
            builder.Property(announcement => announcement.Title).IsRequired();
            builder.Property(announcement => announcement.Title)
                   .HasMaxLength(2 * Announcement.MaxTitleSize);
            builder.Property(announcement => announcement.Title)
                   .HasConversion<StringHexConverter>();
            builder.Property(announcement => announcement.Content)
                   .HasMaxLength(2 * Announcement.MaxContentSize);
            builder.Property(announcement => announcement.Content)
                   .HasConversion<StringHexConverter>();
            builder.ToTable(nameof(Announcement));
        }
    }
}