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
            db.Roles.AddRange(new Role[]
          {
                new Role
                {
                    Id=1,
                    Name="Role1",
                    IsDeleted=false,
                },
                new Role
                {
                    Id=2,
                    Name="Role2",
                    IsDeleted=true,
                }
          });

            db.Users.AddRange(new User[]
             {
                new User
                {
                    Id=1,
                    Name="user with no access",
                    Email="fail@test.com",
                    IsAdmin=false,
                    IsDeleted=false,
                    RoleId=null,
                },
                new User
                {
                    Id=2,
                    Name="user with  access",
                    Email="success@test.com",
                    IsAdmin=false,
                    IsDeleted=false,
                    RoleId=1,
                }
             });

            db.SaveChanges();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}
