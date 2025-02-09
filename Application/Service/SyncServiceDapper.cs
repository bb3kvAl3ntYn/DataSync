using Common.DTO;
using DataSync.Application.Interfaces;
using Domain;
using Dapper;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;


namespace DataSync.Application.Service
{
    public class SyncServiceDapper : ISyncService
    {
        private readonly IDbConnection _sourceDbConnection;
        private readonly IDbConnection _targetDbConnection;

        public SyncServiceDapper(IConfiguration configuration)
        {
            _sourceDbConnection = new SqlConnection(configuration.GetConnectionString("MsSqlConnection"));
            _targetDbConnection = new SqliteConnection(configuration.GetConnectionString("SQLiteConnection"));
        }

        public async Task SyncDataAsync()
        {
            if (_sourceDbConnection.State != ConnectionState.Open)
                _sourceDbConnection.Open();
            if (_targetDbConnection.State != ConnectionState.Open)
                _targetDbConnection.Open();
            using var sourceTransaction =  _sourceDbConnection.BeginTransaction();
            using var targetTransaction = _targetDbConnection.BeginTransaction();

            try
            {
                var sourceCustomers = await _sourceDbConnection.QueryAsync<Customer>(
            "SELECT * FROM Customers", transaction: sourceTransaction);

                foreach (var sourceCustomer in sourceCustomers)
                {
                    // Sync Customer Data
                      var targetCustomer = await _targetDbConnection.QueryFirstOrDefaultAsync<Customer>(
                "SELECT * FROM Customers WHERE CustomerID = @CustomerID", 
                new { sourceCustomer.CustomerID }, transaction: targetTransaction);

                    if (targetCustomer == null)
                    {
                        await _targetDbConnection.ExecuteAsync(
                            "INSERT INTO Customers (CustomerID, Name, Email, Phone) VALUES (@CustomerID, @Name, @Email, @Phone);",
                            new
                            {
                                sourceCustomer.CustomerID,
                                sourceCustomer.Name,
                                sourceCustomer.Email,
                                sourceCustomer.Phone
                            }, transaction: targetTransaction);

                        await LogChangesAsync("Customer", sourceCustomer.CustomerID, "New customer added", targetTransaction);
                    }
                    else
                    {
                        if (HasCustomerChanged(sourceCustomer, targetCustomer))
                        {
                            await TrackCustomerChangesAsync(targetCustomer, sourceCustomer, targetTransaction);
                            await _targetDbConnection.ExecuteAsync(
                                "UPDATE Customers SET Name = @Name, Email = @Email, Phone = @Phone WHERE CustomerID = @CustomerID;",
                                new
                                {
                                    sourceCustomer.Name,
                                    sourceCustomer.Email,
                                    sourceCustomer.Phone,
                                    sourceCustomer.CustomerID
                                }, transaction: targetTransaction);
                        }
                    }

                    // Sync Locations for the Customer
                    await SyncLocationsAsync(sourceCustomer, targetCustomer, targetTransaction,sourceTransaction);
                }

                await _targetDbConnection.ExecuteAsync(
                    "INSERT INTO SyncLogs (SyncTimestamp, Operation, Details) VALUES (@SyncTimestamp, @Operation, @Details);",
                    new { SyncTimestamp = DateTime.UtcNow, Operation = "SYNC", Details = "Full sync completed" },
                    transaction: targetTransaction);

                targetTransaction.Commit();
                sourceTransaction.Commit();
                _sourceDbConnection.Close();
                _targetDbConnection.Close();
            }
            catch (Exception ex)
            {
                targetTransaction.Rollback();
                sourceTransaction.Rollback();

                await _targetDbConnection.ExecuteAsync(
                    "INSERT INTO SyncLogs (SyncTimestamp, Operation, Details) VALUES (@SyncTimestamp, @Operation, @Details);",
                    new { SyncTimestamp = DateTime.UtcNow, Operation = "ERROR", Details = $"Sync failed: {ex.Message}" },
                    transaction: targetTransaction);
                _sourceDbConnection.Close();
                _targetDbConnection.Close();
                throw;
            }
        }

        private async Task SyncLocationsAsync(Customer sourceCustomer, Customer targetCustomer, IDbTransaction targetTransaction, IDbTransaction sourceTransaction)
        {
            // Retrieve locations for the source customer
            var sourceLocations = await _sourceDbConnection.QueryAsync<Location>(
                "SELECT * FROM Locations WHERE CustomerID = @CustomerID;", new { sourceCustomer.CustomerID },transaction: sourceTransaction);

            foreach (var sourceLocation in sourceLocations)
            {
                var targetLocation = await _targetDbConnection.QueryFirstOrDefaultAsync<Location>(
                    "SELECT * FROM Locations WHERE LocationID = @LocationID;", new { sourceLocation.LocationID });

                if (targetLocation == null)
                {
                    await _targetDbConnection.ExecuteAsync(
                        "INSERT INTO Locations (LocationID, CustomerID, Address) VALUES (@LocationID, @CustomerID, @Address);",
                        new
                        {
                            sourceLocation.LocationID,
                            sourceCustomer.CustomerID,
                            sourceLocation.Address
                        }, transaction: targetTransaction);

                    await LogChangesAsync("Location", sourceLocation.LocationID, "New location added", targetTransaction);
                }
                else if (targetLocation.Address != sourceLocation.Address)
                {
                    await TrackLocationChangesAsync(targetLocation, sourceLocation, targetTransaction);
                    await _targetDbConnection.ExecuteAsync(
                        "UPDATE Locations SET Address = @Address WHERE LocationID = @LocationID;",
                        new
                        {
                            sourceLocation.Address,
                            sourceLocation.LocationID
                        }, transaction: targetTransaction);
                }
            }
        }

        private bool HasCustomerChanged(Customer source, Customer target)
        {
            return source.Name != target.Name ||
                   source.Email != target.Email ||
                   source.Phone != target.Phone;
        }

        private async Task TrackCustomerChangesAsync(Customer oldCustomer, Customer newCustomer, IDbTransaction targetTransaction)
        {
            if (oldCustomer.Name != newCustomer.Name)
                await TrackChangeAsync("Customer", oldCustomer.CustomerID, "Name", oldCustomer.Name, newCustomer.Name, targetTransaction);

            if (oldCustomer.Email != newCustomer.Email)
                await TrackChangeAsync("Customer", oldCustomer.CustomerID, "Email", oldCustomer.Email, newCustomer.Email, targetTransaction);

            if (oldCustomer.Phone != newCustomer.Phone)
                await TrackChangeAsync("Customer", oldCustomer.CustomerID, "Phone", oldCustomer.Phone, newCustomer.Phone, targetTransaction);
        }

        private async Task TrackLocationChangesAsync(Location oldLocation, Location newLocation, IDbTransaction targetTransaction)
        {
            if (oldLocation.Address != newLocation.Address)
                await TrackChangeAsync("Location", oldLocation.LocationID, "Address", oldLocation.Address, newLocation.Address, targetTransaction);
        }

        private async Task TrackChangeAsync(string tableName, int recordId, string fieldName, string oldValue, string newValue, IDbTransaction targetTransaction)
        {
            await _targetDbConnection.ExecuteAsync(
                "INSERT INTO ChangeTracker (TableName, RecordID, FieldName, OldValue, NewValue, ChangeTimestamp) VALUES (@TableName, @RecordID, @FieldName, @OldValue, @NewValue, @ChangeTimestamp);",
                new
                {
                    TableName = tableName,
                    RecordID = recordId,
                    FieldName = fieldName,
                    OldValue = oldValue,
                    NewValue = newValue,
                    ChangeTimestamp = DateTime.UtcNow
                }, transaction: targetTransaction);
        }

        private async Task LogChangesAsync(string tableName, int recordId, string details, IDbTransaction targetTransaction)
        {
            await _targetDbConnection.ExecuteAsync(
                "INSERT INTO SyncLogs (SyncTimestamp, Operation, Details) VALUES (@SyncTimestamp, @Operation, @Details);",
                new
                {
                    SyncTimestamp = DateTime.UtcNow,
                    Operation = tableName,
                    Details = $"{details} (ID: {recordId})"
                }, transaction: targetTransaction);
        }

        public async Task<List<CustomerDto>> GetCustomerData()
        {
            var query = @"SELECT 
                            c.CustomerID, 
                            c.Name, 
                            c.Email, 
                            c.Phone, 
                            GROUP_CONCAT(l.Address, ', ') AS Locations
                        FROM Customers c
                        LEFT JOIN Locations l ON c.CustomerID = l.CustomerID
                        GROUP BY c.CustomerID;;";

            var customerDtos = await _targetDbConnection.QueryAsync<CustomerDto>(
                query);

            return customerDtos.ToList();
        }
        public async Task<List<LocationDto>> GetCustomerLocationsAsync(int customerId)
        {
            var query = @"
                        SELECT 
                            l.LocationID, 
                            l.Address
                        FROM Locations l
                        WHERE l.CustomerID = @CustomerID;";

            var locationDtos = await _targetDbConnection.QueryAsync<LocationDto>(
                query, new { CustomerID = customerId });

            return locationDtos.ToList();
        }
    }

}
