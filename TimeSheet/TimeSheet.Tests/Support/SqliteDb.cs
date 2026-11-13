using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using TimeSheet.DataProviders.Repository;
using TimeSheet.Models;

namespace TimeSheet.Tests.Support
{
    /// <summary>
    /// Contexto de teste: reproduz a regra de login e e-mail sem diferenciar maiusculas/minusculas
    /// com NOCASE, equivalente a collation CI aplicada pelo Context em producao (SQL Server).
    /// </summary>
    public class SqliteContext : Context
    {
        public SqliteContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Credential>().Property(c => c.Login).UseCollation("NOCASE");
            modelBuilder.Entity<User>().Property(u => u.Email).UseCollation("NOCASE");
        }
    }

    /// <summary>Banco SQLite em memoria compartilhado entre varios contextos (simula requisicoes distintas).</summary>
    public sealed class SqliteDb : IDisposable
    {
        private readonly SqliteConnection _connection;

        public SqliteDb()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            using var context = NewContext();
            context.Database.EnsureCreated();
        }

        public Context NewContext()
            => new SqliteContext(new DbContextOptionsBuilder<SqliteContext>().UseSqlite(_connection).Options);

        public int AddUser(string name, string email, string login, string password)
        {
            using var context = NewContext();

            var user = new User
            {
                Name = name,
                Email = email,
                Credential = new Credential { Login = login, Password = password }
            };

            context.User.Add(user);
            context.SaveChanges();

            return user.Id;
        }

        public void Dispose() => _connection.Dispose();
    }
}
