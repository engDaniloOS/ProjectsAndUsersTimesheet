using Microsoft.EntityFrameworkCore;
using System.Linq;
using TimeSheet.Models;

namespace TimeSheet.DataProviders.Repository
{
    public class Context : DbContext
    {
        /// <summary>
        /// Collation explícita (insensível a maiúsculas/minúsculas) das colunas com unicidade, para que
        /// o índice único e as comparações não dependam da collation padrão do servidor.
        /// </summary>
        public const string CaseInsensitiveCollation = "SQL_Latin1_General_CP1_CI_AS";

        public Context(DbContextOptions options) : base(options) { }

        public DbSet<Credential> Credentials { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<WorkTime> WorkTime { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Credential>().HasKey(c => c.Id);
            modelBuilder.Entity<Credential>().Property(c => c.Login).IsRequired().HasMaxLength(450);
            modelBuilder.Entity<Credential>().HasIndex(c => c.Login).IsUnique();

            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<User>().Property(u => u.Name).IsRequired().HasMaxLength(450);
            modelBuilder.Entity<User>().Property(u => u.Email).IsRequired().HasMaxLength(450);
            modelBuilder.Entity<User>().HasIndex(u => u.Name);
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().HasMany(u => u.Projects).WithMany(p => p.Users);
            modelBuilder.Entity<User>().HasOne(u => u.Credential);

            modelBuilder.Entity<Project>().HasKey(p => p.Id);
            modelBuilder.Entity<Project>().HasIndex(p => p.Title);
            modelBuilder.Entity<Project>().HasMany(p => p.Users).WithMany(u => u.Projects);

            modelBuilder.Entity<WorkTime>().HasKey(w => w.Id);
            modelBuilder.Entity<WorkTime>().HasIndex(w => w.ProjectId);
            modelBuilder.Entity<WorkTime>().HasIndex(w => w.UserId);

            // Collation específica do SQL Server; outros provedores (ex.: SQLite nos testes) definem a sua.
            if (Database.IsSqlServer())
            {
                modelBuilder.Entity<Credential>().Property(c => c.Login).UseCollation(CaseInsensitiveCollation);
                modelBuilder.Entity<User>().Property(u => u.Email).UseCollation(CaseInsensitiveCollation);
            }
        }
    }
}
