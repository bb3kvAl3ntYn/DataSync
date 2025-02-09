using Common.DTO;
using DataSync.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        public ISyncService _syncService { get; set; }
        public CustomerController(ISyncService syncService)
        {
                _syncService = syncService;
        }
        [HttpGet]
        public async Task<ActionResult<List<CustomerDto>>> GetCustomers()
        {
            var customers = await _syncService.GetCustomerData();
            return Ok(customers);
        }

        [HttpGet("{customerId}/locations")]
        public async Task<ActionResult<List<LocationDto>>> GetCustomerLocations(int customerId)
        {
            var locations = await _syncService.GetCustomerLocationsAsync(customerId);

            if (locations == null || locations.Count == 0)
                return NotFound($"No locations found for customer ID {customerId}");

            return Ok(locations);
        }
    }
}
