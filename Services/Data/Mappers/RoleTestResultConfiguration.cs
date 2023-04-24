using HomemadeLMS.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomemadeLMS.Services.Data
{
    public class RoleTestResultConfiguration : IEntityTypeConfiguration<RoleTestResult>
    {
        public void Configure(EntityTypeBuilder<RoleTestResult> builder)
        {
            builder.HasKey(result => result.Username);
            builder.Property(result => result.Username).ValueGeneratedNever();
            builder.Property(result => result.Username).HasMaxLength(Account.MaxUsernameSize);
            builder.ToTable(nameof(RoleTestResult));
        }
    }
}