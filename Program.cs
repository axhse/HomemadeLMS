using HomemadeLMS.Environment;
using HomemadeLMS.Services;

namespace HomemadeLMS
{
    public static class Program
    {
        private static readonly ConfigurationGroup configuration;

        static Program()
        {
            configuration = Builder.BuildConfiguration(AppliedSpec.Configuration);
            SecretManager = Builder.BuildSecretManager(AppliedSpec.SecretManager);
            MailingService = new(configuration[ComponentName.Mailer], configuration[ComponentName.Host]);
        }

        public static void Main()
        {
            var app = Application.Builder.Build(configuration[ComponentName.Application]);
            app.Run();
        }

        public static MailingService MailingService { get; private set; }
        public static SecretManager SecretManager { get; private set; }
    }
}