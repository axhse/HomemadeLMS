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
            builder.Property(account => account.Username).ValueGeneratedNever();
            builder.Property(account => account.Username).HasMaxLength(Account.MaxUsernameSize);
            builder.Property(account => account.HeadUsername).HasMaxLength(Account.MaxUsernameSize);
            builder.Property(account => account.PasswordHash).HasMaxLength(Account.PasswordHashSize);
            builder.Property(account => account.PasswordHash).IsFixedLength(true);
            builder.Property(account => account.Role).IsRequired();
            builder.ToTable("Account");
        }
    }
}