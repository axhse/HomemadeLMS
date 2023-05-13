using HomemadeLMS.Controllers;
using HomemadeLMS.Environment;

namespace HomemadeLMS.Application
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate next;

        public ExceptionHandler(RequestDelegate next) => this.next = next;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                var logger = LoggerBuilder.Build(nameof(ExceptionHandler));
                logger.LogError(new EventId(), exception, "Unexpected exception.");
                context.Response.Redirect(HomeController.ErrorPath);
            }
        }
    }
}