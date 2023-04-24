using HomemadeLMS.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomemadeLMS.Services.Data
{
    public class CourseMemberConfiguration : IEntityTypeConfiguration<CourseMember>
    {
        public void Configure(EntityTypeBuilder<CourseMember> builder)
        {
            builder.HasKey(member => member.Uid);
            builder.HasIndex(member => member.CourseId);
            builder.HasIndex(member => member.Username);
            builder.Property(member => member.Uid).ValueGeneratedNever();
            builder.Property(member => member.Uid).HasMaxLength(22 + Account.MaxUsernameSize);
            builder.Property(member => member.CourseId).IsRequired();
            builder.Property(member => member.Username).IsRequired();
            builder.Property(member => member.Username).HasMaxLength(Account.MaxUsernameSize);
            builder.Property(member => member.Role).IsRequired();
            builder.ToTable(nameof(CourseMember));
        }
    }
}