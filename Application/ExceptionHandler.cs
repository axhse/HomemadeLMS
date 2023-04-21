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
            catch (Exception)
            {
                context.Response.Redirect(HomeController.ErrorPath);
            }
        }
    }
}