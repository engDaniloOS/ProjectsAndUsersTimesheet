using Microsoft.EntityFrameworkCore;
using System.Linq;
using TimeSheet.Configuration;
using TimeSheet.Tests.Support;
using Xunit;

namespace TimeSheet.Tests.DataProviders
{
    public class AdminSeedTests : System.IDisposable
    {
        private readonly SqliteDb _db = new SqliteDb();

        public void Dispose() => _db.Dispose();

        [Fact]
        public void EnsureAdminUser_EmptyDatabase_CreatesTheAdmin()
        {
            using var context = _db.NewContext();

            DatabaseManagementService.EnsureAdminUser(context);

            var admin = context.User.Include(u => u.Credential).Single();
            Assert.Equal("admin", admin.Credential.Login);
            Assert.Equal("admin", admin.Credential.Password);
            Assert.Equal("admin@vibbra.com.br", admin.Email);
        }

        [Fact]
        public void EnsureAdminUser_CalledTwice_KeepsASingleAdmin()
        {
            using (var first = _db.NewContext()) DatabaseManagementService.EnsureAdminUser(first);
            using (var second = _db.NewContext()) DatabaseManagementService.EnsureAdminUser(second);

            using var context = _db.NewContext();
            Assert.Equal(1, context.User.Count());
            Assert.Equal(1, context.Credentials.Count());
        }

        [Fact]
        public void EnsureAdminUser_AdminAlreadyExistsInAnotherCase_DoesNotCreateAnother()
        {
            _db.AddUser("Administrator", "root@x.com", "Admin", "whatever");

            using var context = _db.NewContext();
            DatabaseManagementService.EnsureAdminUser(context);

            Assert.Equal(1, context.User.Count());
        }
    }
}
