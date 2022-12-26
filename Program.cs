using HomemadeLMS.Application;

namespace HomemadeLMS
{
    public static class Program
    {
        public static readonly AppConfig AppConfig = AppConfigBuilder.BuildConfig();

        public static void Main()
        {
            var app = AppBuilder.BuildApp(AppConfig);
            app.Run();
        }
    }
}