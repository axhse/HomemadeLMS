using HomemadeLMS.Models.Domain;
using HomemadeLMS.Services.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace HomemadeLMS.Application
{
    public sealed class App
    {
        private readonly AppConfig appConfig;
        private readonly WebApplication webApplication;

        public App(AppConfig appConfig, WebApplication webApplication)
        {
            this.appConfig = appConfig;
            this.webApplication = webApplication;
        }

        public AppConfig AppConfig => appConfig;

        public void Run() => webApplication.Run();
    }

    public static class AppBuilder
    {
        public static App BuildApp(AppConfig appConfig)
        {
            var builder = WebApplication.CreateBuilder();
            ConfigureWebServices(builder.Services, appConfig.BuildingConfig);
            ConfigureDomainServices(builder.Services);
            var webApplication = builder.Build();
            ConfigureWebApp(webApplication, appConfig.BuildingConfig);
            return new App(appConfig, webApplication);
        }

        private static void ConfigureWebServices(
            IServiceCollection services, BuildingConfig config)
        {
            services.AddControllersWithViews();
            if (config.IsHttpsForced)
            {
                services.AddHsts(options => { options.MaxAge = TimeSpan.FromDays(365); });
            }

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(
                options =>
                {
                    options.Cookie.Name = "AuthToken";
                }
            );
        }

        private static void ConfigureDomainServices(
            IServiceCollection services)
        {
            services.AddScoped<IStorage<string, Account>>(
                _ => new Storage<string, Account>(new AccountContext())
            );
            services.AddScoped<IStorage<int, Course>>(
                _ => new Storage<int, Course>(new CourseContext())
            );
            services.AddScoped<IStorage<int, CourseMember>>(
                _ => new Storage<int, CourseMember>(new CourseMemberContext())
            );
            services.AddScoped(_ => new CourseAggregator(new CourseCompositeContext()));
        }

        private static void ConfigureWebApp(WebApplication app, BuildingConfig config)
        {
            if (config.IsDevelopmentExceptionHandlerEnabled)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseMiddleware<ExceptionHandler>();
            }

            if (config.IsHttpsForced)
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRouting();
            app.UseEndpoints(endpoints => new Router().ConfigureEndpoints(endpoints));
        }
    }
}