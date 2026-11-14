using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimeSheet.Configuration;

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

            ApiBehaviorServiceConfig.Configure(services);

            SwaggerServiceConfig.Configure(services);

            DatabaseServiceConfig.Configure(services, Configuration);
            
            MapperServiceConfig.Configure(services);

            DependencyInjectionServiceConfig.Configure(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TimeSheet v1"));

            app.UseAuthentication();

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            DatabaseManagementService.MigrationInitializer(app);
        }
    }
}
