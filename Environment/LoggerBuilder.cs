namespace HomemadeLMS.Environment
{
    public class LoggerBuilder
    {
        public static ILogger Build(string name)
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