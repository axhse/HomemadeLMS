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

    public class ServiceConfig
    {
        private string? managerToken;

        public ServiceConfig(bool hasDemoContent, string? managerToken = null)
        {
            this.managerToken = managerToken;
            HasDemoContent = hasDemoContent;
        }

        public bool HasDemoContent { get; private set; }

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
            var builderConfigRoot = configRoot.GetSection("Builder");
            var hasDevExceptionHandler = builderConfigRoot.GetValue<bool>("HasDevExceptionHandler");
            var isHttpsForced = builderConfigRoot.GetValue<bool>("IsHttpsForced");
            var builderConfig = new BuilderConfig(hasDevExceptionHandler, isHttpsForced);

            var databaseConfigRoot = configRoot.GetSection("Database");
            var connectionString = databaseConfigRoot.GetValue<string>("ConnectionString");
            var databaseConfig = new DatabaseConfig(connectionString);

            var serviceConfigRoot = configRoot.GetSection("Service");
            var hasDemoContent = serviceConfigRoot.GetValue<bool>("HasDemoContent");
            var managerToken = serviceConfigRoot.GetValue<string?>("ManagerToken");
            var serviceConfig = new ServiceConfig(hasDemoContent, managerToken);

            return new AppConfig(builderConfig, databaseConfig, serviceConfig);
        }
    }
}