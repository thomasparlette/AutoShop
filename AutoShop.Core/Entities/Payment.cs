using System;

namespace AutoShop.Core.Entities
{
    public class Payment
    {
        public int Id { get; set; }

        public int WorkOrderId { get; set; }
        public WorkOrder? WorkOrder { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public decimal AmountPaid { get; set; }

        public string PaymentMethod { get; set; } = "Cash";

        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }
}
