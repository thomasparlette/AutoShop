using System;
using AutoShop.Core.Enums;


namespace AutoShop.Core.Entities
{
    public class Appointment
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        public int? WorkOrderId { get; set; }
        public WorkOrder? WorkOrder { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public string? Subject { get; set; }
        public string? Notes { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    }
}
