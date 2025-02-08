using DataSync.Application.Interfaces;
using DataSync.Application.Service;
using Infrastructure.AppDbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SQLitePCL;
using System.IO;
using System.Windows;

namespace DataSyncApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public App()
        {
            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) 
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddDbContext<SqlServerDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("MsSqlConnection")));

            services.AddDbContext<SQLLiteDbContext>(options =>
               options.UseSqlite(configuration.GetConnectionString("SQLiteConnection")));
            services.AddSingleton<IConfiguration>(configuration);
            services.AddTransient<ISyncService,SyncServiceDapper>();
            services.AddSingleton<MainWindow>();
            ServiceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }


}
