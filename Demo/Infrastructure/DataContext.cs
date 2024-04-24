using Demo.Domain;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Demo.Infrastructure
{
    public class DataContext : DbContext
    {

        public DataContext()
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // in memory database used for simplicity, change to a real db for production applications
            options.UseInMemoryDatabase("TestDb");


        }

        public static void SeedData()
        {
            var db = new DataContext();
            db.SaveChanges();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<UserProfileHint>  Hints { get; set; }
    }
}
