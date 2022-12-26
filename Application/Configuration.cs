namespace HomemadeLMS.Application
{
    public struct AppConfig
    {
        public BuildingConfig BuildingConfig;
        public DatabaseConfig DatabaseConfig;
    }

    public struct BuildingConfig
    {
        public bool IsDevelopmentExceptionHandlerEnabled;
        public bool IsHttpsForced;

        public BuildingConfig()
        {
            IsDevelopmentExceptionHandlerEnabled = false;
            IsHttpsForced = false;
        }
    }

    public struct DatabaseConfig
    {
        public string DbConnectionString;

        public DatabaseConfig(string dbConnectionString)
        {
            DbConnectionString = dbConnectionString;
        }
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
            var buildingConfigRoot = configRoot.GetSection("Building");
            var buildingConfig = new BuildingConfig
            {
                IsDevelopmentExceptionHandlerEnabled
                    = buildingConfigRoot.GetValue<bool>("IsDevelopmentExceptionHandlerEnabled"),
                IsHttpsForced = buildingConfigRoot.GetValue<bool>("IsHttpsForced"),
            };
            var databaseConfig = new DatabaseConfig(
                buildingConfigRoot.GetValue<string>("DbConnectionString"));
            return new AppConfig
            {
                BuildingConfig = buildingConfig,
                DatabaseConfig = databaseConfig,
            };
        }
    }
}