using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.SeedData
{
    public static class SeedData
    {
        public static void SeedCustomer(this ModelBuilder builder)
        {
            builder.Entity<Customer>().HasData(
                new Customer { CustomerID = 1, Name = "John Doe", Email = "john.doe@example.com", Phone = "123-456-7890" },
                new Customer { CustomerID = 2, Name = "Jane Smith", Email = "jane.smith@example.com", Phone = "234-567-8901" },
                new Customer { CustomerID = 3, Name = "Michael Johnson", Email = "michael.johnson@example.com", Phone = "345-678-9012" },
                new Customer { CustomerID = 4, Name = "Emily Davis", Email = "emily.davis@example.com", Phone = "456-789-0123" },
                new Customer { CustomerID = 5, Name = "Robert Wilson", Email = "robert.wilson@example.com", Phone = "567-890-1234" },
                new Customer { CustomerID = 6, Name = "Jessica Brown", Email = "jessica.brown@example.com", Phone = "678-901-2345" },
                new Customer { CustomerID = 7, Name = "David Miller", Email = "david.miller@example.com", Phone = "789-012-3456" },
                new Customer { CustomerID = 8, Name = "Sarah Taylor", Email = "sarah.taylor@example.com", Phone = "890-123-4567" },
                new Customer { CustomerID = 9, Name = "Christopher Martinez", Email = "christopher.martinez@example.com", Phone = "901-234-5678" },
                new Customer { CustomerID = 10, Name = "Amanda Anderson", Email = "amanda.anderson@example.com", Phone = "012-345-6789" }
            );
        }

        public static void SeedLocation(this ModelBuilder builder)
        {
            builder.Entity<Location>().HasData(
                new Location { LocationID = 1, CustomerID = 1, Address = "123 Main St, City A" },
                new Location { LocationID = 2, CustomerID = 1, Address = "456 Oak St, City A" },
                new Location { LocationID = 3, CustomerID = 2, Address = "789 Pine St, City B" },
                new Location { LocationID = 4, CustomerID = 2, Address = "101 Maple St, City B" },
                new Location { LocationID = 5, CustomerID = 3, Address = "202 Cedar St, City C" },
                new Location { LocationID = 6, CustomerID = 3, Address = "303 Birch St, City C" },
                new Location { LocationID = 7, CustomerID = 4, Address = "404 Walnut St, City D" },
                new Location { LocationID = 8, CustomerID = 4, Address = "505 Chestnut St, City D" },
                new Location { LocationID = 9, CustomerID = 5, Address = "606 Spruce St, City E" },
                new Location { LocationID = 10, CustomerID = 5, Address = "707 Redwood St, City E" },
                new Location { LocationID = 11, CustomerID = 6, Address = "808 Elm St, City F" },
                new Location { LocationID = 12, CustomerID = 6, Address = "909 Cypress St, City F" },
                new Location { LocationID = 13, CustomerID = 7, Address = "1010 Fir St, City G" },
                new Location { LocationID = 14, CustomerID = 7, Address = "1111 Beech St, City G" },
                new Location { LocationID = 15, CustomerID = 8, Address = "1212 Dogwood St, City H" },
                new Location { LocationID = 16, CustomerID = 8, Address = "1313 Magnolia St, City H" },
                new Location { LocationID = 17, CustomerID = 9, Address = "1414 Aspen St, City I" },
                new Location { LocationID = 18, CustomerID = 9, Address = "1515 Willow St, City I" },
                new Location { LocationID = 19, CustomerID = 10, Address = "1616 Hickory St, City J" },
                new Location { LocationID = 20, CustomerID = 10, Address = "1717 Sycamore St, City J" }
            );
        }

    }
}
