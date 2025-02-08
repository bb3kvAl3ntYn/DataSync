using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class CustomerDto
    {
        public int CustomerID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Locations { get; set; }
    }
    public class LocationDto
    {
        public int LocationID { get; set; }
        public string Address { get; set; }
    }
}
