using System;
using System.Collections.Generic;

namespace AutoShop.Core.Entities;

public class WorkOrderInspection
{
    public int Id { get; set; }

    public int WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    public string? TechnicianName { get; set; }
    public string? OverallNotes { get; set; }

    public ICollection<WorkOrderInspectionItem> Items { get; set; } = new List<WorkOrderInspectionItem>();
}