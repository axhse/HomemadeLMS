using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.Services.Data
{
    public class AccountDbClient : DbClient<Account>
    {
        public AccountDbClient() : base(new AccountConfiguration())
        { }
    }
}