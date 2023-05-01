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
            ConfigureWebServices(builder.Services, appConfig.BuilderConfig);
            ConfigureDomainServices(builder.Services);
            var webApplication = builder.Build();
            ConfigureWebApp(webApplication, appConfig.BuilderConfig);
            return new App(appConfig, webApplication);
        }

        private static void ConfigureWebServices(IServiceCollection services, BuilderConfig config)
        {
            services.AddControllersWithViews();
            if (config.IsHttpsForced)
            {
                services.AddHsts(options => { options.MaxAge = TimeSpan.FromDays(365); });
            }

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options => { options.Cookie.Name = "AuthToken"; });
        }

        private static void ConfigureDomainServices(IServiceCollection services)
        {
            AddStorage<int, Announcement>(services, () => new AnnouncementContext());
            AddStorage<int, Course>(services, () => new CourseContext());
            AddStorage<int, Homework>(services, () => new HomeworkContext());
            AddStorage<int, Team>(services, () => new TeamContext());
            AddStorage<string, Account>(services, () => new AccountContext());
            AddStorage<string, CourseMember>(services, () => new CourseMemberContext());
            AddStorage<string, HomeworkStatus>(services, () => new HomeworkStatusContext());
            AddStorage<string, RoleTestResult>(services, () => new RoleTestResultContext());

            services.AddScoped(_ => new EntityAggregator(new CompositeContext()));
        }

        private static void ConfigureWebApp(WebApplication app, BuilderConfig config)
        {
            if (config.HasDevExceptionHandler)
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

            app.UseRouting();
            app.UseEndpoints(endpoints => Router.ConfigureEndpoints(endpoints));
        }

        private static void AddStorage<TKey, TValue>(IServiceCollection services,
            Func<GenericContext<TValue>> source) where TValue : class
        {
            services.AddScoped<IStorage<TKey, TValue>>(_ => new Storage<TKey, TValue>(source()));
        }
    }
}