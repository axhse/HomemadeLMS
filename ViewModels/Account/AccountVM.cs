using HomemadeLMS.Models.Domain;

namespace HomemadeLMS.ViewModels
{
    public class AccountVM
    {
        public AccountVM(Account requestMaker, Account targetAccount)
        {
            RequestMaker = requestMaker;
            TargetAccount = targetAccount;
        }

        public Account RequestMaker { get; private set; }
        public Account TargetAccount { get; private set; }
    }
}