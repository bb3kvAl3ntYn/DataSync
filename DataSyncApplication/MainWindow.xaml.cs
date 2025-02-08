using DataSync.Application.Interfaces;
using DataSync.Application.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows;

namespace DataSyncApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow :Window
    {
        private readonly ISyncService _syncService;
        private System.Timers.Timer _syncTimer;

        public MainWindow()
        {
            _syncService = App.ServiceProvider.GetRequiredService<ISyncService>();
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            _syncTimer = new System.Timers.Timer();
            _syncTimer.Elapsed += async (s, e) => await SyncData();
        }

        private void btnSetInterval_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtInterval.Text, out int interval) && interval > 0)
            {
                _syncTimer.Interval = interval * 1000; // Convert to milliseconds
                _syncTimer.Start();
                LogMessage($"Sync interval set to {interval} seconds");
            }
            else
            {
                MessageBox.Show("Please enter a valid interval in seconds");
            }
        }

        private async void btnManualSync_Click(object sender, RoutedEventArgs e)
        {
            await SyncData();
        }

        private async Task SyncData()
        {
            try
            {
                await _syncService.SyncDataAsync();
                await LoadCustomers();
                LogMessage("Sync completed successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"Sync failed: {ex.Message}");
            }
        }

        private async Task LoadCustomers()
        {
            try
            {
                var customers = await _syncService.GetCustomerData();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    dgCustomers.ItemsSource = customers;
                });
            }
            catch (Exception ex)
            {
                LogMessage($"Failed to load customers: {ex.Message}");
            }
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                txtLog.Text = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n" + txtLog.Text;
            });
        }
    }
}