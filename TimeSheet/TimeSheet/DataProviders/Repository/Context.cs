using Microsoft.EntityFrameworkCore;
using System.Linq;
using TimeSheet.Models;

namespace TimeSheet.DataProviders.Repository
{
    public class Context : DbContext
    {
        public Context(DbContextOptions options) : base(options) { }

        public DbSet<Credential> Credentials { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<WorkTime> WorkTime { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Credential>().HasKey(c => c.Id);

            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<User>().HasIndex(u => u.Name);
            modelBuilder.Entity<User>().HasIndex(u => u.Email);
            modelBuilder.Entity<User>().HasMany(u => u.Projects).WithMany(p => p.Users);
            modelBuilder.Entity<User>().HasOne(u => u.Credential);

            modelBuilder.Entity<Project>().HasKey(p => p.Id);
            modelBuilder.Entity<Project>().HasIndex(p => p.Title);
            modelBuilder.Entity<Project>().HasMany(p => p.Users).WithMany(u => u.Projects);

            modelBuilder.Entity<WorkTime>().HasKey(w => w.Id);
            modelBuilder.Entity<WorkTime>().HasIndex(w => w.ProjectId);
            modelBuilder.Entity<WorkTime>().HasIndex(w => w.UserId);
        }
    }
}
