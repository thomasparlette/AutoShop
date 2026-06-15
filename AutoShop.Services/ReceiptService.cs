using AutoShop.Core.Entities;
using System.Text;

namespace AutoShop.Services;

public class ReceiptService
{
    public string BuildReceiptText(WorkOrder workOrder)
    {
        var sb = new StringBuilder();

        sb.AppendLine("AUTO SHOP RECEIPT");
        sb.AppendLine("----------------------------------------");
        sb.AppendLine($"Work Order #: {workOrder.WorkOrderNumber}");
        sb.AppendLine($"Date: {workOrder.CreatedAt:g}");
        sb.AppendLine();

        sb.AppendLine("CUSTOMER");
        sb.AppendLine($"{workOrder.Customer?.FullName}");
        sb.AppendLine($"{workOrder.Customer?.Phone}");
        sb.AppendLine();

        sb.AppendLine("VEHICLE");
        sb.AppendLine($"{workOrder.Vehicle?.Year} {workOrder.Vehicle?.Make} {workOrder.Vehicle?.Model}");
        sb.AppendLine($"VIN: {workOrder.Vehicle?.Vin}");
        sb.AppendLine($"Plate: {workOrder.Vehicle?.LicensePlate}");
        sb.AppendLine();

        sb.AppendLine("COMPLAINT");
        sb.AppendLine(workOrder.Complaint ?? string.Empty);
        sb.AppendLine();

        sb.AppendLine("LINE ITEMS");
        foreach (var item in workOrder.LineItems)
        {
            sb.AppendLine($"{item.ItemType,-6} {item.Description,-25} Qty:{item.Quantity:N2}  Unit:{item.UnitPrice:C}  Total:{item.LineTotal:C}");
        }

        sb.AppendLine();
        sb.AppendLine("----------------------------------------");
        sb.AppendLine($"Labor Total:   {workOrder.LaborTotal:C}");
        sb.AppendLine($"Parts Total:   {workOrder.PartsTotal:C}");
        sb.AppendLine($"Tax:           {workOrder.TaxTotal:C}");
        sb.AppendLine($"Discount:      {workOrder.DiscountTotal:C}");
        sb.AppendLine($"Grand Total:   {workOrder.GrandTotal:C}");
        sb.AppendLine($"Amount Paid:   {workOrder.AmountPaid:C}");
        sb.AppendLine($"Balance Due:   {workOrder.BalanceDue:C}");
        sb.AppendLine("----------------------------------------");

        if (!string.IsNullOrWhiteSpace(workOrder.Notes))
        {
            sb.AppendLine("NOTES");
            sb.AppendLine(workOrder.Notes);
        }

        return sb.ToString();
    }
}