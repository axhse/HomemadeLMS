using HomemadeLMS.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomemadeLMS.Services.Data
{
    public class HomeworkStatusConfiguration : IEntityTypeConfiguration<HomeworkStatus>
    {
        public void Configure(EntityTypeBuilder<HomeworkStatus> builder)
        {
            builder.HasKey(status => status.Uid);
            builder.HasIndex(status => status.HomeworkId);
            builder.HasIndex(status => status.SubjectId);
            builder.Property(status => status.Uid).ValueGeneratedNever();
            builder.Property(status => status.Uid).HasMaxLength(HomeworkStatus.MaxUidSize);
            builder.Property(status => status.HomeworkId).IsRequired();
            builder.Property(status => status.SubjectId).IsRequired();
            builder.Property(status => status.SubjectId)
                   .HasMaxLength(HomeworkStatus.MaxSubjectIdSize);
            builder.Property(status => status.SubmitUsername).HasMaxLength(Account.MaxUsernameSize);
            builder.Property(status => status.EvaluatorUsername)
                   .HasMaxLength(Account.MaxUsernameSize);
            builder.ToTable(nameof(HomeworkStatus));
        }
    }
}