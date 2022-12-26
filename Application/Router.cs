namespace HomemadeLMS.Application
{
    public sealed class Router
    {
        public void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapControllers();
            endpoints.MapControllerRoute(
                name: "RedirectToSpecificHomepage",
                pattern: "/{controller=Home}/{*any}",
                defaults: new { Controller = "Home", Action = "RedirectToHomepage" }
            );
            endpoints.MapControllerRoute(
                name: "RedirectToGlobalHomepage",
                pattern: "{*any}",
                defaults: new { Controller = "Base", Action = "RedirectToHomepage" }
            );
        }
    }
}