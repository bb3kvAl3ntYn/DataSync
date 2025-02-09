using DataSync.Application.Interfaces;
using DataSync.Application.Service;
using Infrastructure.AppDbContexts;
using Microsoft.EntityFrameworkCore;

namespace DataSyncAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configuration = builder.Configuration;

            builder.Services.AddDbContext<SqlServerDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("MsSqlConnection")));

            builder.Services.AddDbContext<SQLLiteDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("SQLiteConnection")));

            builder.Services.AddTransient<ISyncService, SyncServiceDapper>();


               builder.Services.AddControllers();

            var app = builder.Build();


            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
