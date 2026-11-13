using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TimeSheet.DataProviders.Repository;
using TimeSheet.Models;
using TimeSheet.Tests.Support;
using Xunit;

namespace TimeSheet.Tests.DataProviders
{
    public class UserRepositoryTests : System.IDisposable
    {
        private readonly SqliteDb _db = new SqliteDb();

        public void Dispose() => _db.Dispose();

        private UserRepository NewRepository() => new UserRepository(_db.NewContext());

        private static User Edited(string login, string password, string email = "ana@x.com") => new User
        {
            Name = "Ana Edited",
            Email = email,
            Credential = new Credential { Login = login, Password = password }
        };

        // ---------- ExistsLogin / ExistsEmail ----------

        [Fact]
        public async Task ExistsLogin_ExistingLogin_ReturnsTrue()
        {
            _db.AddUser("Ana", "ana@x.com", "ana", "secret");

            Assert.True(await NewRepository().ExistsLogin("ana", null));
        }

        [Fact]
        public async Task ExistsLogin_DifferentCase_ReturnsTrue()
        {
            _db.AddUser("Ana", "ana@x.com", "ana", "secret");

            Assert.True(await NewRepository().ExistsLogin("ANA", null));
        }

        [Fact]
        public async Task ExistsLogin_UnknownLogin_ReturnsFalse()
        {
            _db.AddUser("Ana", "ana@x.com", "ana", "secret");

            Assert.False(await NewRepository().ExistsLogin("bia", null));
        }

        [Fact]
        public async Task ExistsLogin_IgnoringTheOwnerId_ReturnsFalse()
        {
            var id = _db.AddUser("Ana", "ana@x.com", "ana", "secret");

            Assert.False(await NewRepository().ExistsLogin("ana", id));
        }

        [Fact]
        public async Task ExistsLogin_IgnoringAnotherUsersId_ReturnsTrue()
        {
            var id = _db.AddUser("Ana", "ana@x.com", "ana", "secret");

            Assert.True(await NewRepository().ExistsLogin("ana", id + 100));
        }

        [Fact]
        public async Task ExistsEmail_ExistingEmailInAnotherCase_ReturnsTrue()
        {
            _db.AddUser("Ana", "ana@x.com", "ana", "secret");

            Assert.True(await NewRepository().ExistsEmail("ANA@X.COM", null));
        }

        [Fact]
        public async Task ExistsEmail_UnknownEmail_ReturnsFalse()
        {
            _db.AddUser("Ana", "ana@x.com", "ana", "secret");

            Assert.False(await NewRepository().ExistsEmail("bia@x.com", null));
        }

        [Fact]
        public async Task ExistsEmail_IgnoringTheOwnerId_ReturnsFalse()
        {
            var id = _db.AddUser("Ana", "ana@x.com", "ana", "secret");

            Assert.False(await NewRepository().ExistsEmail("ana@x.com", id));
        }

        // ---------- Indices unicos (garantia final do banco) ----------

        [Fact]
        public async Task SaveNewUser_SameLoginInAnotherCase_ViolatesUniqueIndex()
        {
            _db.AddUser("Ana", "ana@x.com", "ana", "secret");

            var duplicate = new User { Name = "Outra", Email = "outra@x.com", Credential = new Credential { Login = "ANA", Password = "p" } };

            await Assert.ThrowsAnyAsync<DbUpdateException>(() => NewRepository().SaveNewUser(duplicate));
        }

        [Fact]
        public async Task SaveNewUser_SameEmailInAnotherCase_ViolatesUniqueIndex()
        {
            _db.AddUser("Ana", "ana@x.com", "ana", "secret");

            var duplicate = new User { Name = "Outra", Email = "ANA@X.COM", Credential = new Credential { Login = "outra", Password = "p" } };

            await Assert.ThrowsAnyAsync<DbUpdateException>(() => NewRepository().SaveNewUser(duplicate));
        }

        [Fact]
        public async Task EditUser_ToAnotherUsersLogin_ViolatesUniqueIndex()
        {
            _db.AddUser("Ana", "ana@x.com", "ana", "secret");
            var biaId = _db.AddUser("Bia", "bia@x.com", "bia", "secret");

            await Assert.ThrowsAnyAsync<DbUpdateException>(
                () => NewRepository().EditUser(biaId, Edited("ana", null, "bia@x.com")));
        }

        // ---------- Edicao e senha opcional ----------

        [Fact]
        public async Task EditUser_NullPassword_KeepsTheCurrentPassword()
        {
            var id = _db.AddUser("Ana", "ana@x.com", "ana", "secret");

            var result = await NewRepository().EditUser(id, Edited("ana2", password: null));

            Assert.Equal(id, result.Id);

            using var context = _db.NewContext();
            var stored = await context.User.Include(u => u.Credential).SingleAsync(u => u.Id == id);
            Assert.Equal("secret", stored.Credential.Password);
            Assert.Equal("ana2", stored.Credential.Login);
            Assert.Equal("Ana Edited", stored.Name);
        }

        [Fact]
        public async Task EditUser_WithPassword_ReplacesThePassword()
        {
            var id = _db.AddUser("Ana", "ana@x.com", "ana", "secret");

            await NewRepository().EditUser(id, Edited("ana", password: "new-secret"));

            using var context = _db.NewContext();
            var stored = await context.User.Include(u => u.Credential).SingleAsync(u => u.Id == id);
            Assert.Equal("new-secret", stored.Credential.Password);
        }

        [Fact]
        public async Task EditUser_MissingUser_ReturnsEmptyUser()
        {
            var result = await NewRepository().EditUser(999, Edited("x", "y"));

            Assert.Equal(0, result.Id);
        }

        [Fact]
        public async Task EditUser_KeepingOwnLoginAndEmail_DoesNotConflict()
        {
            var id = _db.AddUser("Ana", "ana@x.com", "ana", "secret");

            var result = await NewRepository().EditUser(id, Edited("ana", password: null));

            Assert.Equal(id, result.Id);
        }
    }
}
