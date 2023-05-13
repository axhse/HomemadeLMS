using HomemadeLMS.Environment;

namespace HomemadeLMS.Services
{
    public class AuthService
    {
        private readonly object syncRoot = new();
        private readonly string domainName;
        private readonly MailingService mailingService;
        private readonly TokenService tokenService;

        public AuthService(ConfigurationComponent authServiceConfiguration,
                              ConfigurationComponent hostConfiguration)
        {
            domainName = hostConfiguration.GetString(PropertyName.DomainName);
            mailingService = new(authServiceConfiguration);
            var lifetime = authServiceConfiguration.GetInt(PropertyName.RequestLifetimeInMinutes);
            tokenService = new(TimeSpan.FromMinutes(lifetime));
        }

        public async Task CreateRequest(string emailAddress)
        {
            string token;
            lock (syncRoot)
            {
                token = tokenService.Add(emailAddress);
            }
            var confirmationUrl = $"{domainName}/signin/confirm?token={token}";
            await mailingService.SendConfirmationMail(emailAddress, confirmationUrl);
        }

        public string? GetEmailAddress(string? token)
        {
            lock (syncRoot)
            {
                return tokenService.FetchId(token);
            }
        }
    }
}