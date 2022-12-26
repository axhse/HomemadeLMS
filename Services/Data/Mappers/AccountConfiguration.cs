using HomemadeLMS.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomemadeLMS.Services.Data
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(account => account.Username);
            builder.HasIndex(account => account.Username).IsUnique();
            builder.Property(account => account.Username).IsRequired();
            builder.Property(account => account.Username).ValueGeneratedNever();
            builder.Property(account => account.Username).HasMaxLength(100);
            builder.ToTable("Account");
        }
    }
}