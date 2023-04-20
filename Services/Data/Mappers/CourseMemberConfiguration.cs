using HomemadeLMS.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomemadeLMS.Services.Data
{
    public class CourseMemberConfiguration : IEntityTypeConfiguration<CourseMember>
    {
        public void Configure(EntityTypeBuilder<CourseMember> builder)
        {
            builder.HasKey(courseMember => courseMember.RecordId);
            builder.HasIndex(courseMember => courseMember.CourseId);
            builder.HasIndex(courseMember => courseMember.Username);
            builder.Property(courseMember => courseMember.CourseId).IsRequired();
            builder.Property(courseMember => courseMember.CourseId).ValueGeneratedNever();
            builder.Property(courseMember => courseMember.Username).IsRequired();
            builder.Property(courseMember => courseMember.Username).ValueGeneratedNever();
            builder.Property(courseMember => courseMember.Username).HasMaxLength(100);
            builder.Property(courseMember => courseMember.Role).IsRequired();
            builder.ToTable("CourseMember");
        }
    }
}