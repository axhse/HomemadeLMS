using HomemadeLMS.Environment;
using HomemadeLMS.Services;

namespace HomemadeLMS.Application
{
    public sealed class Application
    {
        private readonly WebApplication webApplication;

        public Application(WebApplication webApplication) : this(webApplication, new())
        { }

        public Application(WebApplication webApplication, ConfigurationComponent configuration)
        {
            this.webApplication = webApplication;
            Configuration = configuration;
        }

        public ConfigurationComponent Configuration { get; set; }

        public void Run()
        {
            if (Configuration.Get(PropertyName.HasDemoContent, defaultValue: false))
            {
                var contentGenerator = new DemoContentGenerator();
                contentGenerator.CleanAllContent();
                contentGenerator.GenerateContent();
            }
            webApplication.Run();
        }
    }
}