namespace HomemadeLMS.Application
{
    public struct AppConfig
    {
        public AppConfig(BuilderConfig builderConfig, DatabaseConfig databaseConfig)
        {
            BuilderConfig = builderConfig;
            DatabaseConfig = databaseConfig;
        }

        public BuilderConfig BuilderConfig { get; set; }
        public DatabaseConfig DatabaseConfig { get; set; }
    }

    public struct BuilderConfig
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

    public struct DatabaseConfig
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

            return new AppConfig(builderConfig, databaseConfig);
        }
    }
}