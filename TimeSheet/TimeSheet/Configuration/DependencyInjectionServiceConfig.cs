using Microsoft.Extensions.DependencyInjection;
using TimeSheet.DataProviders.Repository;
using TimeSheet.Domain;
using TimeSheet.Domain.Converters;
using TimeSheet.Domain.UseCases;

namespace TimeSheet.Configuration
{
    public static class DependencyInjectionServiceConfig
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddTransient<IWorkTimeService, WorkTimeService>();
            services.AddTransient<IWorkTimeRepository, WorkTimeRepository>();
            services.AddTransient<IWorkTimeConverter, WortTimeConverter>();

            services.AddTransient<ILogin, LoginService>();
            services.AddTransient<ICredentialRepository, CredentialRepository>();
            services.AddTransient<ILoginConverter, LoginConverter>();

            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUserConverter, UserConverter>();

            services.AddTransient<IProjectService, ProjectService>();
            services.AddTransient<IProjectRepository, ProjectRepository>();
            services.AddTransient<IProjectConverter, ProjectConverter>();
        }
    }
}
