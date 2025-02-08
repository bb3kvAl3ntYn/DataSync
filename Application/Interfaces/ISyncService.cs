﻿
using Common.DTO;

namespace DataSync.Application.Interfaces
{
    public interface ISyncService
    {
        Task SyncDataAsync();
        Task<List<CustomerDto>> GetCustomerData();
    }
}
