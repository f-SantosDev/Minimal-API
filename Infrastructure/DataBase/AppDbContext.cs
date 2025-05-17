using Microsoft.EntityFrameworkCore;
using Minimal_API_Project.Domain.Entities;

namespace Minimal_API_Project.Infrastructure.DataBase
{
    public class AppDbContext : DbContext
    {
        private readonly IConfiguration _configurationAppSettings;
        public AppDbContext(IConfiguration configurationAppSettings)
        {
            _configurationAppSettings = configurationAppSettings;
        }
        public DbSet<Administrator> Administrator { get; set; } // DbSet to create Database table for administrator
        public DbSet<Vehicle> Vehicle { get; set; } // DbSet to create Database table for vehicle

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Administrator>().HasData(
                new Administrator
                {
                    Id = 1,
                    Email = "administrator@teste.com",
                    Password = "123456",
                    Profile = "Admin"
                }
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var conectionString = _configurationAppSettings.GetConnectionString("MySQL")?.ToString();

                if (!string.IsNullOrEmpty(conectionString))
                {
                    optionsBuilder.UseMySql(conectionString, ServerVersion.AutoDetect(conectionString));
                    return;
                }
            
                throw new Exception("Connection string not found");
            }
            
        }
    }
}
