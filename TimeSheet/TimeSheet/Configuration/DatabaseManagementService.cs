using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TimeSheet.DataProviders.Repository;
using TimeSheet.Models;

namespace TimeSheet.Configuration
{
    public static class DatabaseManagementService
    {
        public static void MigrationInitializer(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var context = serviceScope.ServiceProvider.GetService<Context>();

            context.Database.Migrate();

            InitializeAdminUser(context);
        }

        private static void InitializeAdminUser(Context context)
        {
            var user = new User
            {
                Credential = new Credential { Login = "admin", Password = "admin" },
                Email = "admin@vibbra.com.br",
                Name = "admin"
            };

            context.User.Add(user);
            context.SaveChanges();
        }
    }
}
