using System;
using System.Collections.Generic;
using AutoShop.Core.Enums;

namespace AutoShop.Core.Entities
{
    public class WorkOrder
    {
        public int Id { get; set; }

        public string WorkOrderNumber { get; set; } = string.Empty;

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }

        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Draft;

        public string? Complaint { get; set; }
        public string? Diagnosis { get; set; }
        public string? Notes { get; set; }
        public int? MileageIn { get; set; }
        public int? MileageOut { get; set; }
        public decimal LaborTotal { get; set; }
        public decimal PartsTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal BalanceDue { get; set; }
        public WorkOrderInspection? Inspection { get; set; }
        public ICollection<WorkOrderLineItem> LineItems { get; set; } = new List<WorkOrderLineItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public string StatusDisplay => Status switch
        {
            WorkOrderStatus.Draft => "DRAFT",
            WorkOrderStatus.Open => "OPEN",
            WorkOrderStatus.InProgress => "IN PROGRESS",
            WorkOrderStatus.WaitingApproval => "WAITING APPROVAL",
            WorkOrderStatus.Completed => "COMPLETED",
            WorkOrderStatus.Paid => "PAID",
            WorkOrderStatus.Closed => "CLOSED",
            WorkOrderStatus.Cancelled => "CANCELLED",
            _ => Status.ToString()
        };
        public int? TechnicianId { get; set; }
        public Technician? Technician { get; set; }
        public bool InventoryApplied { get; set; }
    }
}
