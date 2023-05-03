namespace HomemadeLMS.Application
{
    public class AppConfig
    {
        public AppConfig(BuilderConfig builderConfig, DatabaseConfig databaseConfig,
                         ServiceConfig serviceConfig)
        {
            BuilderConfig = builderConfig;
            DatabaseConfig = databaseConfig;
            ServiceConfig = serviceConfig;
        }

        public BuilderConfig BuilderConfig { get; set; }
        public DatabaseConfig DatabaseConfig { get; set; }
        public ServiceConfig ServiceConfig { get; set; }
    }

    public class BuilderConfig
    {
        public BuilderConfig(bool hasDevExceptionHandler)
        {
            HasDevExceptionHandler = hasDevExceptionHandler;
        }

        public bool HasDevExceptionHandler { get; set; }
    }


    public class MailingServiceConfig
    {
        public MailingServiceConfig(string apiDomain, string apiKey)
        {
            ApiDomain = apiDomain;
            ApiKey = apiKey;
        }

        public string ApiDomain { get; set; }
        public string ApiKey { get; set; }
    }

    public class ServiceConfig
    {
        private string? managerToken;

        public ServiceConfig(bool hasDemoContent,
                             string? hostUrlBase = null,
                             string? managerToken = null,
                             MailingServiceConfig? mailingServiceConfig = null)
        {
            this.managerToken = managerToken;
            HasDemoContent = hasDemoContent;
            HostUrlBase = hostUrlBase;
            MailingServiceConfig = mailingServiceConfig;
        }

        public bool HasDemoContent { get; set; }
        public string? HostUrlBase { get; set; }
        public MailingServiceConfig? MailingServiceConfig { get; set; }

        public string? ManagerToken => managerToken;

        public void DeleteManagerToken()
        {
            managerToken = null;
        }
    }

    public class DatabaseConfig
    {
        public DatabaseConfig(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; set; }
    }

    public static class AppConfigBuilder
    {
        public static AppConfig BuildConfig()
        {
            var baseConfigRoot = new ConfigurationBuilder()
               .AddJsonFile($"appsettings.json", false, false)
               .Build();
            string? environmentName = baseConfigRoot.GetValue<string?>("Environment");
            var envConfigRoot = new ConfigurationBuilder()
               .AddJsonFile($"appsettings.{environmentName}.json", false, false)
               .Build();
            return ParseConfigRoot(envConfigRoot);
        }

        private static AppConfig ParseConfigRoot(IConfigurationRoot configRoot)
        {
            var builderConfigRoot = configRoot.GetSection(nameof(BuilderConfig));
            var hasDevExceptionHandler = builderConfigRoot.GetValue(
                nameof(BuilderConfig.HasDevExceptionHandler), false
            );
            var builderConfig = new BuilderConfig(hasDevExceptionHandler);

            var databaseConfigRoot = configRoot.GetSection(nameof(DatabaseConfig));
            var connectionString = databaseConfigRoot.GetValue<string?>(
                nameof(DatabaseConfig.ConnectionString)
            );
            if (connectionString is null)
            {
                var errorMessage = nameof(DatabaseConfig.ConnectionString) + " must be specified.";
                throw new ArgumentException(errorMessage);
            }
            var databaseConfig = new DatabaseConfig(connectionString);

            var serviceConfigRoot = configRoot.GetSection(nameof(ServiceConfig));
            var hasDemoContent = serviceConfigRoot.GetValue(
                nameof(ServiceConfig.HasDemoContent), false
            );
            var hostUrlBase = serviceConfigRoot.GetValue<string?>(nameof(ServiceConfig.HostUrlBase));
            var managerToken = serviceConfigRoot.GetValue<string?>(nameof(ServiceConfig.ManagerToken));
            var mailingApiDomain = serviceConfigRoot.GetValue<string?>("MailingApiDomain");
            var mailingApiKey = serviceConfigRoot.GetValue<string?>("MailingApiKey");
            var serviceConfig = new ServiceConfig(hasDemoContent, hostUrlBase, managerToken);
            if (mailingApiDomain is not null && mailingApiKey is not null)
            {
                serviceConfig.MailingServiceConfig = new(mailingApiDomain, mailingApiKey);
            }

            return new AppConfig(builderConfig, databaseConfig, serviceConfig);
        }
    }
}