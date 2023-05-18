using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using TimeSheet.DataProviders.Repository;

namespace TimeSheet.Configuration
{
    public static class DatabaseServiceConfig
    {
        public static void Configure(IServiceCollection services, IConfiguration configuration)
        {
            var isRunningOnContainer = bool.Parse(Environment.GetEnvironmentVariable("Database_OnDocker") ?? "false");

            if (isRunningOnContainer)
                services.AddDbContext<Context>(option => option.UseSqlServer(GetConnectionStringDocker(configuration)));
            else
                services.AddDbContext<Context>(option => option.UseSqlServer(GetConnectionStringLocal(configuration)));
        }

        private static string GetConnectionStringLocal(IConfiguration configuration)
            => configuration.GetConnectionString("Local").Replace("?path?", Environment.CurrentDirectory);

        private static string GetConnectionStringDocker(IConfiguration configuration)
            => configuration.GetConnectionString("Docker");

    }
}
