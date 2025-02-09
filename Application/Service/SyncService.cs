using Common.DTO;
using DataSync.Application.Interfaces;
using Domain;
using Infrastructure.AppDbContexts;
using Microsoft.EntityFrameworkCore;

namespace DataSync.Application.Service
{
    public class SyncService : ISyncService
    {
        private readonly SqlServerDbContext _sourceDb;
        private readonly SQLLiteDbContext _targetDb;

        public SyncService(SqlServerDbContext sourceDb, SQLLiteDbContext targetDb)
        {
            _sourceDb = sourceDb;
            _targetDb = targetDb;
        }

        public async Task SyncDataAsync()
        {
            using var sourceTransaction = await _sourceDb.Database.BeginTransactionAsync();
            using var targetTransaction = await _targetDb.Database.BeginTransactionAsync();

            try
            {
                var sourceCustomers = await _sourceDb.Customers
                    .Include(c => c.Locations)
                    .AsNoTracking()
                    .ToListAsync();

                foreach (var sourceCustomer in sourceCustomers)
                {
                    var targetCustomer = await _targetDb.Customers
                        .Include(c => c.Locations)
                        .FirstOrDefaultAsync(c => c.CustomerID == sourceCustomer.CustomerID);

                    if (targetCustomer == null)
                    {
                        _targetDb.Customers.Add(sourceCustomer);
                        await LogChangesAsync("Customer", sourceCustomer.CustomerID, "New customer added");
                    }
                    else
                    {
                        if (HasCustomerChanged(sourceCustomer, targetCustomer))
                        {
                            await TrackCustomerChangesAsync(targetCustomer, sourceCustomer);
                            _targetDb.Entry(targetCustomer).CurrentValues.SetValues(sourceCustomer);
                        }

                        await SyncLocationsAsync(sourceCustomer, targetCustomer);
                    }
                }

                _targetDb.SyncLogs.Add(new SyncLog
                {
                    SyncTimestamp = DateTime.UtcNow,
                    Operation = "SYNC",
                    Details = "Full sync completed"
                });

                await _targetDb.SaveChangesAsync();
                await targetTransaction.CommitAsync();
                await sourceTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await targetTransaction.RollbackAsync();
                await sourceTransaction.RollbackAsync();

                _targetDb.SyncLogs.Add(new SyncLog
                {
                    SyncTimestamp = DateTime.UtcNow,
                    Operation = "ERROR",
                    Details = $"Sync failed: {ex.Message}"
                });
                await _targetDb.SaveChangesAsync();

                throw;
            }
        }

        private async Task SyncLocationsAsync(Customer sourceCustomer, Customer targetCustomer)
        {
            foreach (var sourceLocation in sourceCustomer.Locations)
            {
                var targetLocation = targetCustomer.Locations
                    .FirstOrDefault(l => l.LocationID == sourceLocation.LocationID);

                if (targetLocation == null)
                {
                    targetCustomer.Locations.Add(sourceLocation);
                    await LogChangesAsync("Location", sourceLocation.LocationID, "New location added");
                }
                else if (targetLocation.Address != sourceLocation.Address)
                {
                    await TrackLocationChangesAsync(targetLocation, sourceLocation);
                    _targetDb.Entry(targetLocation).CurrentValues.SetValues(sourceLocation);
                }
            }
        }

        private bool HasCustomerChanged(Customer source, Customer target)
        {
            return source.Name != target.Name ||
                   source.Email != target.Email ||
                   source.Phone != target.Phone;
        }

        private async Task TrackCustomerChangesAsync(Customer oldCustomer, Customer newCustomer)
        {
            if (oldCustomer.Name != newCustomer.Name)
                await TrackChangeAsync("Customer", oldCustomer.CustomerID, "Name", oldCustomer.Name, newCustomer.Name);

            if (oldCustomer.Email != newCustomer.Email)
                await TrackChangeAsync("Customer", oldCustomer.CustomerID, "Email", oldCustomer.Email, newCustomer.Email);

            if (oldCustomer.Phone != newCustomer.Phone)
                await TrackChangeAsync("Customer", oldCustomer.CustomerID, "Phone", oldCustomer.Phone, newCustomer.Phone);
        }

        private async Task TrackLocationChangesAsync(Location oldLocation, Location newLocation)
        {
            if (oldLocation.Address != newLocation.Address)
                await TrackChangeAsync("Location", oldLocation.LocationID, "Address", oldLocation.Address, newLocation.Address);
        }

        private async Task TrackChangeAsync(string tableName, int recordId, string fieldName, string oldValue, string newValue)
        {
            _targetDb.ChangeTracker.Add(new ChangeTracker
            {
                TableName = tableName,
                RecordID = recordId,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                ChangeTimestamp = DateTime.UtcNow
            });
        }

        private async Task LogChangesAsync(string tableName, int recordId, string details)
        {
            _targetDb.SyncLogs.Add(new SyncLog
            {
                SyncTimestamp = DateTime.UtcNow,
                Operation = tableName,
                Details = $"{details} (ID: {recordId})"
            });
        }

        public async Task<List<CustomerDto>> GetCustomerData()
        {
            var customerDtos = _targetDb.Customers
                                .Include(c => c.Locations) 
                                .Select(c => new CustomerDto
                                {
                                    CustomerID = c.CustomerID,
                                    Name = c.Name,
                                    Email = c.Email,
                                    Phone = c.Phone
                                })
                                .ToList();
            return customerDtos;
        }
        public async Task<List<LocationDto>> GetCustomerLocationsAsync(int customerId)
        {
            return await _targetDb.Locations
                .Where(l => l.CustomerID == customerId)
                .Select(l => new LocationDto
                {
                    LocationID = l.LocationID,
                    Address = l.Address
                })
                .ToListAsync();
        }
    }
}
