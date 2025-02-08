using Domain;
using Infrastructure.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.AppDbContexts
{
    public class SqlServerDbContext : DbContext
    {
        public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Location> Locations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasKey(e => e.LocationID);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(200);

                entity.HasOne(d => d.Customer)
                      .WithMany(p => p.Locations)
                      .HasForeignKey(d => d.CustomerID);
            });
            modelBuilder.SeedCustomer();
            modelBuilder.SeedLocation();    
        }
    }
    public class SqlServerDbContextFactory : IDesignTimeDbContextFactory<SqlServerDbContext>
    {
        public SqlServerDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SqlServerDbContext>();

            // Use a connection string from a configuration file
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=DataSync;Trusted_Connection=True;");

            return new SqlServerDbContext(optionsBuilder.Options);
        }
    }
}
