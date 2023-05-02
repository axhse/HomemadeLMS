using HomemadeLMS.Application;
using HomemadeLMS.Services;

namespace HomemadeLMS
{
    public static class Program
    {
        public static readonly AppConfig AppConfig = AppConfigBuilder.BuildConfig();
        public static readonly ILogger Logger = BuildLogger(nameof(Program));

        public static void Main()
        {
            if (AppConfig.ServiceConfig.HasDemoContent)
            {
                var contentGenerator = new DemoContentGenerator();
                contentGenerator.CleanAllContent();
                contentGenerator.GenerateContent();
            }

            var app = AppBuilder.BuildApp(AppConfig);
            app.Run();
        }

        public static ILogger BuildLogger(string name)
        {
            var factory = LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole(options =>
                {
                    options.TimestampFormat = "[HH:mm:ss]  ";
                });
            });
            return factory.CreateLogger(name);
        }
    }
}