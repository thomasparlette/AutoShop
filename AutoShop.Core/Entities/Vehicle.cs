using System;
using System.Collections.Generic;
using System.Text;

namespace AutoShop.Core.Entities
{
    public class Vehicle
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int? Year { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string? Trim { get; set; }

        public string? Vin { get; set; }
        public string? LicensePlate { get; set; }
        public string? Color { get; set; }
        public int? Mileage { get; set; }
        public int? MileageOut { get; set; }

        public string? Notes { get; set; }

        public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
