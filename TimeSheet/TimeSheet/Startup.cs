using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using TimeSheet.Configuration;
using TimeSheet.DataProviders.Repository;

namespace TimeSheet
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            AuthenticatorServiceConfig.Configure(services, Configuration);

            services.AddControllers();

            SwaggerServiceConfig.Configure(services);

            DatabaseServiceConfig.Configure(services, Configuration);
            
            MapperServiceConfig.Configure(services);

            DependencyInjectionServiceConfig.Configure(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TimeSheet v1"));
            }

            app.UseAuthentication();

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            DatabaseManagementService.MigrationInitializer(app);
        }
    }
}
