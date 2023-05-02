using HomemadeLMS.Controllers;

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
                Program.Logger.LogError(new EventId(), exception, "Unexpected exception.");
                context.Response.Redirect(HomeController.ErrorPath);
            }
        }
    }
}