using HomemadeLMS.Environment;
using HomemadeLMS.Models.Domain;
using HomemadeLMS.Services.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace HomemadeLMS.Application
{
    public static class Builder
    {
        public static Application Build(ConfigurationComponent configuration)
        {
            var builder = WebApplication.CreateBuilder();
            ConfigureWebServices(builder.Services);
            ConfigureDomainServices(builder.Services);
            var webApplication = builder.Build();
            ConfigureWebApplication(webApplication, configuration);
            return new Application(webApplication, configuration);
        }

        private static void ConfigureWebServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddHsts(options => { options.MaxAge = TimeSpan.FromDays(365); });

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

        private static void ConfigureWebApplication(WebApplication app, ConfigurationComponent configuration)
        {
            if (configuration.Get(PropertyName.HasDevExceptionHandler, defaultValue: false))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseMiddleware<ExceptionHandler>();
            }

            app.UseHsts();
            app.UseHttpsRedirection();

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