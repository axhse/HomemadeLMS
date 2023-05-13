namespace HomemadeLMS.Environment
{
    public static class AppliedSpec
    {
        public static ConfigurationSpec Configuration
        {
            get
            {
                var applicationComponentSpec = new ComponentSpec(ComponentName.Application)
                {
                    Properties = new()
                    {
                        new(PropertyType.Bool, PropertyName.HasDemoContent, isRequired: false),
                        new(PropertyType.Bool, PropertyName.HasDevExceptionHandler, isRequired: false),
                    }
                };
                var authServiceComponentSpec = new ComponentSpec(ComponentName.AuthService)
                {
                    Properties = new()
                    {
                        new(PropertyType.String, PropertyName.ApiEmailAddress, isRequired: true),
                        new(PropertyType.Int, PropertyName.ApiTimeoutInSeconds, isRequired: true),
                        new(PropertyType.Int, PropertyName.RequestLifetimeInMinutes, isRequired: true),
                    }
                };
                var hostComponentSpec = new ComponentSpec(ComponentName.Host)
                {
                    Properties = new()
                    {
                        new(PropertyType.String, PropertyName.DomainName, isRequired: true),
                    }
                };
                var configurationSpec = new ConfigurationSpec()
                {
                    Components = new()
                    {
                        applicationComponentSpec,
                        authServiceComponentSpec,
                        hostComponentSpec,
                    }
                };
                return configurationSpec;
            }
        }

        public static SecretManagerSpec SecretManager
        {
            get
            {
                var secretManagerSpec = new SecretManagerSpec()
                {
                    Secrets = new()
                    {
                        new(SecretName.DatabaseConnectionString, isRequired: true),
                        new(SecretName.MailingApiKey, isRequired: true),
                        new(SecretName.ManagerToken, isRequired: true),
                    }
                };
                return secretManagerSpec;
            }
        }
    }
}