using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public class AccountAndObject<TObject>
    {
        public AccountAndObject(Account account, TObject obj)
        {
            Account = account;
            Object = obj;
        }

        public Account Account { get; private set; }
        public TObject Object { get; private set; }
    }
}