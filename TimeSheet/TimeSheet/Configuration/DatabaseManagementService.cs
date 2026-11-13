using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using TimeSheet.DataProviders.Repository;
using TimeSheet.Models;

namespace TimeSheet.Configuration
{
    public static class DatabaseManagementService
    {
        private const string AdminLogin = "admin";

        public static void MigrationInitializer(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var context = serviceScope.ServiceProvider.GetService<Context>();

            context.Database.Migrate();

            EnsureAdminUser(context);
        }

        /// <summary>
        /// Cria o usuário administrador inicial somente se ainda não existir um com login "admin"
        /// (a comparação segue a collation da coluna: não diferencia maiúsculas de minúsculas).
        /// </summary>
        public static void EnsureAdminUser(Context context)
        {
            if (context.User.Any(u => u.Credential.Login == AdminLogin))
                return;

            var user = new User
            {
                Credential = new Credential { Login = AdminLogin, Password = "admin" },
                Email = "admin@vibbra.com.br",
                Name = "admin"
            };

            context.User.Add(user);
            context.SaveChanges();
        }
    }
}
