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
        public BuilderConfig()
        {
            HasDevExceptionHandler = false;
            IsHttpsForced = false;
        }

        public BuilderConfig(bool hasDevExceptionHandler, bool isHttpsForced)
        {
            HasDevExceptionHandler = hasDevExceptionHandler;
            IsHttpsForced = isHttpsForced;
        }

        public bool HasDevExceptionHandler { get; set; }
        public bool IsHttpsForced { get; set; }
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

        public ServiceConfig(bool hasDemoContent, string selfBaseUrl,
                             MailingServiceConfig mailingServiceConfig, string? managerToken = null)
        {
            this.managerToken = managerToken;
            HasDemoContent = hasDemoContent;
            SelfBaseUrl = GetUrlWithFixedProtocol(selfBaseUrl);
            MailingServiceConfig = mailingServiceConfig;
        }

        public bool HasDemoContent { get; private set; }
        public string SelfBaseUrl { get; private set; }
        public MailingServiceConfig MailingServiceConfig { get; private set; }

        public string? ManagerToken => managerToken;

        public void DeleteManagerToken()
        {
            managerToken = null;
        }

        private static string GetUrlWithFixedProtocol(string url)
        {
            url = url.Replace("http://", "https://");
            if (!url.StartsWith("https://"))
            {
                url = "https://" + url;
            }
            return url;
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
            var builderConfigRoot = configRoot.GetSection("Builder");
            var hasDevExceptionHandler = builderConfigRoot.GetValue<bool>("HasDevExceptionHandler");
            var isHttpsForced = builderConfigRoot.GetValue<bool>("IsHttpsForced");
            var builderConfig = new BuilderConfig(hasDevExceptionHandler, isHttpsForced);

            var databaseConfigRoot = configRoot.GetSection("Database");
            var connectionString = databaseConfigRoot.GetValue<string>("ConnectionString");
            var databaseConfig = new DatabaseConfig(connectionString);

            var serviceConfigRoot = configRoot.GetSection("Service");
            var hasDemoContent = serviceConfigRoot.GetValue<bool>("HasDemoContent");
            var selfBaseUrl = serviceConfigRoot.GetValue<string>("SelfBaseUrl");
            var mailingApiDomain = serviceConfigRoot.GetValue<string>("MailingApiDomain");
            var mailingApiKey = serviceConfigRoot.GetValue<string>("MailingApiKey");
            var managerToken = serviceConfigRoot.GetValue<string?>("ManagerToken");
            var mailingServiceConfig = new MailingServiceConfig(mailingApiDomain, mailingApiKey);
            var serviceConfig = new ServiceConfig(
                hasDemoContent, selfBaseUrl, mailingServiceConfig, managerToken
            );

            return new AppConfig(builderConfig, databaseConfig, serviceConfig);
        }
    }
}