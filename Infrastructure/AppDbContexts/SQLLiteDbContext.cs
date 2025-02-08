using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.AppDbContexts
{
    public class SQLLiteDbContext : DbContext
    {
        public SQLLiteDbContext(DbContextOptions<SQLLiteDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<SyncLog> SyncLogs { get; set; }
        public DbSet<ChangeTracker> ChangeTracker { get; set; }

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

                entity.HasOne(d => d.Customer).WithMany(p => p.Locations).HasForeignKey(d => d.CustomerID);
            });

            modelBuilder.Entity<SyncLog>(entity =>
            {
                entity.HasKey(e => e.LogID);
                entity.Property(e => e.SyncTimestamp).IsRequired();
                entity.Property(e => e.Operation).IsRequired();
                entity.Property(e => e.Details).IsRequired();
            });

            modelBuilder.Entity<ChangeTracker>(entity =>
            {
                entity.HasKey(e => e.ChangeID);
                entity.Property(e => e.TableName).IsRequired();
                entity.Property(e => e.FieldName).IsRequired();
                entity.Property(e => e.ChangeTimestamp).IsRequired();
            });
        }
    }

public class SQLLiteDbContextFactory : IDesignTimeDbContextFactory<SQLLiteDbContext>
    {
        public SQLLiteDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SQLLiteDbContext>();

            // Replace with your actual SQLite connection string
            optionsBuilder.UseSqlite("Data Source=C:\\sqlite-db\\global.db;");

            return new SQLLiteDbContext(optionsBuilder.Options);
        }
    }

}
