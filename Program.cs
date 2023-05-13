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
            AuthService = new(
                configuration[ComponentName.AuthService], configuration[ComponentName.Host]
            );
        }

        public static void Main()
        {
            var app = Application.Builder.Build(configuration[ComponentName.Application]);
            app.Run();
        }

        public static AuthService AuthService { get; private set; }
        public static SecretManager SecretManager { get; private set; }
    }
}