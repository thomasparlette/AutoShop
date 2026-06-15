using AutoShop.Core.Enums;

namespace AutoShop.Core.Entities;

public class WorkOrderInspectionItem
{
    public int Id { get; set; }

    public int WorkOrderInspectionId { get; set; }
    public WorkOrderInspection? WorkOrderInspection { get; set; }

    public string Section { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public InspectionStatus Status { get; set; } = InspectionStatus.NotInspected;
    public string? Notes { get; set; }
}