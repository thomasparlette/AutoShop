using AutoShop.Core.Entities;
using AutoShop.Core.Enums;
using AutoShop.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AutoShop.MainApp.Services;

public class InvoicePrintService
{
    public string GetDocumentTitle(WorkOrder workOrder)
    {
        var kind = IsReceipt(workOrder) ? "Receipt" : "Quotation";
        return $"{kind} - {workOrder?.WorkOrderNumber ?? "Work Order"}";
    }

    public FlowDocument BuildInvoiceDocument(WorkOrder workOrder)
    {
        var safeWorkOrder = workOrder ?? new WorkOrder
        {
            Customer = new Customer(),
            Vehicle = new Vehicle(),
            Technician = null,
            Inspection = new WorkOrderInspection()
        };

        using var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>());
        var shop = db.ShopSettings.FirstOrDefault() ?? new ShopSettings();

        var doc = ReportFormatting.CreateBaseDocument();

        BuildCopy(doc, safeWorkOrder, shop, internalCopy: true);
        AddPageBreak(doc);
        BuildCopy(doc, safeWorkOrder, shop, internalCopy: false);

        return doc;
    }

    private static void BuildCopy(FlowDocument doc, WorkOrder workOrder, ShopSettings shop, bool internalCopy)
    {
        var docKind = IsReceipt(workOrder) ? "RECEIPT" : "QUOTATION";
        var copyLabel = internalCopy ? "INTERNAL COPY" : "CUSTOMER COPY";

        var orderedItems = GetOrderedLineItems(workOrder);
        var itemPages = ChunkByVariable(orderedItems, 30, 45).ToList();
        if (itemPages.Count == 0)
        {
            itemPages.Add(new List<WorkOrderLineItem>());
        }

        var totalPagesForThisCopy = itemPages.Count + 1; // checklist page is the final page

        for (int i = 0; i < itemPages.Count; i++)
        {
            var isFirstPage = i == 0;
            var isLastItemPage = i == itemPages.Count - 1;

            AppendInvoiceItemPage(
                doc,
                workOrder,
                shop,
                docKind,
                copyLabel,
                itemPages[i],
                i + 1,
                totalPagesForThisCopy,
                isFirstPage,
                isLastItemPage);

            if (!isLastItemPage)
            {
                AddPageBreak(doc);
            }
        }

        AddPageBreak(doc);
        AppendChecklistPage(doc, workOrder, shop, copyLabel, itemPages.Count + 1, totalPagesForThisCopy);
    }

    private static void AddCustomerAndInvoiceInfo(FlowDocument doc, WorkOrder workOrder, ShopSettings shop)
    {
        var table = new Table { CellSpacing = 0 };
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

        var rg = new TableRowGroup();
        table.RowGroups.Add(rg);

        var row = new TableRow
        {
            Cells =
            {
                BlockCell(BuildCustomerBlock(workOrder)),
                BlockCell(BuildInvoiceBlock(workOrder, shop))
            }
        };

        rg.Rows.Add(row);
        doc.Blocks.Add(table);
    }

    private static void AddVehicleTable(FlowDocument doc, WorkOrder workOrder)
    {
        var table = new Table { CellSpacing = 0 };
        table.Columns.Add(new TableColumn { Width = new GridLength(55) });
        table.Columns.Add(new TableColumn { Width = new GridLength(95) });
        table.Columns.Add(new TableColumn { Width = new GridLength(95) });
        table.Columns.Add(new TableColumn { Width = new GridLength(200) });
        table.Columns.Add(new TableColumn { Width = new GridLength(95) });
        table.Columns.Add(new TableColumn { Width = new GridLength(95) });
        table.Columns.Add(new TableColumn { Width = new GridLength(95) });

        var rg = new TableRowGroup();
        table.RowGroups.Add(rg);

        rg.Rows.Add(new TableRow
        {
            Cells =
        {
            ReportFormatting.Cell("YEAR", true),
            ReportFormatting.Cell("MAKE", true),
            ReportFormatting.Cell("MODEL", true),
            ReportFormatting.Cell("VIN", true),
            ReportFormatting.Cell("LICENSE", true),
            ReportFormatting.Cell("MILEAGE IN", true),
            ReportFormatting.Cell("MILEAGE OUT", true)
        }
        });

        rg.Rows.Add(new TableRow
        {
            Cells =
        {
            ReportFormatting.Cell(workOrder.Vehicle?.Year?.ToString() ?? string.Empty),
            ReportFormatting.Cell(workOrder.Vehicle?.Make ?? string.Empty),
            ReportFormatting.Cell(workOrder.Vehicle?.Model ?? string.Empty),
            ReportFormatting.Cell(workOrder.Vehicle?.Vin ?? string.Empty),
            ReportFormatting.Cell(workOrder.Vehicle?.LicensePlate ?? string.Empty),
            ReportFormatting.Cell(workOrder.Vehicle?.Mileage?.ToString() ?? string.Empty),
            ReportFormatting.Cell(workOrder.MileageOut?.ToString() ?? string.Empty),
        }
        });

        doc.Blocks.Add(table);
    }
    private static void AddLineItemsTable(FlowDocument doc, IReadOnlyList<WorkOrderLineItem> items)
    {
        var table = new Table { CellSpacing = 0 };
        table.TextAlignment = TextAlignment.Center;
        table.Columns.Add(new TableColumn { Width = new GridLength(70) });
        table.Columns.Add(new TableColumn { Width = new GridLength(390) });
        table.Columns.Add(new TableColumn { Width = new GridLength(55) });
        table.Columns.Add(new TableColumn { Width = new GridLength(85) });
        table.Columns.Add(new TableColumn { Width = new GridLength(85) });

        var rg = new TableRowGroup();
        table.RowGroups.Add(rg);

        rg.Rows.Add(new TableRow
        {
            Cells =
        {
            ReportFormatting.Cell("TYPE", true),
            ReportFormatting.Cell("DESCRIPTION", true),
            ReportFormatting.Cell("QTY", true),
            ReportFormatting.Cell("UNIT", true),
            ReportFormatting.Cell("TOTAL", true)
        }
        });

        foreach (var item in items)
        {
            rg.Rows.Add(new TableRow
            {
                Cells =
            {
                ReportFormatting.Cell(item.ItemType.ToString().ToUpperInvariant()),
                ReportFormatting.Cell(item.Description ?? string.Empty),
                ReportFormatting.Cell(item.Quantity.ToString("N2")),
                ReportFormatting.Cell(item.UnitPrice.ToString("C")),
                ReportFormatting.Cell(item.LineTotal.ToString("C"))
            }
            });
        }

        doc.Blocks.Add(table);
    }

    private static void AddNotesAndTotalsSection(FlowDocument doc, WorkOrder workOrder)
    {
        var outer = new Table { CellSpacing = 0 };
        outer.Columns.Add(new TableColumn { Width = new GridLength(2, GridUnitType.Star) });
        outer.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

        var rg = new TableRowGroup();
        outer.RowGroups.Add(rg);

        var row = new TableRow();

        var notesCell = new TableCell
        {
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding = new Thickness(6)
        };

        notesCell.Blocks.Add(new Paragraph
        {
            Margin = new Thickness(0),
            FontWeight = FontWeights.Bold,
            Inlines = { new Run("COMPLAINT") }
        });

        if (!string.IsNullOrWhiteSpace(workOrder.Complaint))
            notesCell.Blocks.Add(new Paragraph { Margin = new Thickness(0), Inlines = { new Run(workOrder.Complaint) } });

        notesCell.Blocks.Add(new Paragraph
        {
            Margin = new Thickness(0, 8, 0, 0),
            FontWeight = FontWeights.Bold,
            Inlines = { new Run("NOTES") }
        });

        if (!string.IsNullOrWhiteSpace(workOrder.Notes))
            notesCell.Blocks.Add(new Paragraph { Margin = new Thickness(0), Inlines = { new Run(workOrder.Notes) } });

        row.Cells.Add(notesCell);

        var totalsCell = new TableCell
        {
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding = new Thickness(0)
        };

        var totals = new Table { CellSpacing = 0 };
        totals.Columns.Add(new TableColumn { Width = new GridLength(4, GridUnitType.Star) });
        totals.Columns.Add(new TableColumn { Width = new GridLength(1.3, GridUnitType.Star) });

        var totalsRg = new TableRowGroup();
        totals.RowGroups.Add(totalsRg);

        totalsRg.Rows.Add(LabelValueRow("LABOR AMOUNT", workOrder.LaborTotal.ToString("C")));
        totalsRg.Rows.Add(LabelValueRow("PARTS AMOUNT", workOrder.PartsTotal.ToString("C")));
        totalsRg.Rows.Add(LabelValueRow("TAX", workOrder.TaxTotal.ToString("C")));
        totalsRg.Rows.Add(LabelValueRow("DISCOUNT", workOrder.DiscountTotal.ToString("C")));
        totalsRg.Rows.Add(LabelValueRow("TOTAL CHARGES", workOrder.GrandTotal.ToString("C")));
        totalsRg.Rows.Add(LabelValueRow("AMOUNT PAID", workOrder.AmountPaid.ToString("C")));

        var balanceRow = LabelValueRow("BALANCE DUE", workOrder.BalanceDue.ToString("C"));
        balanceRow.Cells[0].Foreground = Brushes.DarkRed;
        balanceRow.Cells[1].Foreground = Brushes.DarkRed;
        totalsRg.Rows.Add(balanceRow);

        totalsCell.Blocks.Add(totals);
        row.Cells.Add(totalsCell);

        rg.Rows.Add(row);
        doc.Blocks.Add(outer);
    }

    private static void AddAuthorizationBlock(FlowDocument doc)
    {
        doc.Blocks.Add(new Paragraph
        {
            Margin = new Thickness(0, 0, 0, 2),
            FontWeight = FontWeights.Bold,
            Inlines = { new Run("AUTHORIZATION") }
        });

        doc.Blocks.Add(new Paragraph
        {
            Margin = new Thickness(0),
            Inlines = { new Run("I FULLY UNDERSTAND THE PURPOSES OF THE SAFETY DEVICES ON THIS EQUIPMENT AND SPECIFICALLY REQUEST THAT THEY\r\nNOT BE REPAIRED OR REPLACED, AND I ASSUME RESPONSIBILITY FOR AND HOLD YOU HARMLESS FROM ANY INJURY TO\r\nANYONE THAT MAY RESULT THEREFROM..\r\nIMPORTANT - PLEASE NOTE\r\nWhile the manufacturer may warrant the goods sold to the customer, we make no warranties, express or implied, including any implied warranties of\r\nmerchantability or fitness, with respect to such goods.\r\n\r\nNot responsible for loss or damage in case of fire, theft, or any other cause beyond our control.\r\n\r\nI hereby authorize the above repair work to be done along with the necessary material and hereby grant you and your employees permission to\r\noperate the unit as necessary for the purpose of testing and inspection. An express mechanic's lien is hereby acknowledged on above unit to\r\nsecure the amount of repairs thereto.\r\n") }
        });

        doc.Blocks.Add(new Paragraph
        {
            Margin = new Thickness(0, 6, 0, 0),
            Inlines =
            {
                new Run("X _____________________________________________    DATE _____________________________")
            }
        });
    }

    private static void AddInspectionMetaTable(FlowDocument doc, WorkOrder workOrder)
    {
        var table = new Table { CellSpacing = 0 };
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

        var rg = new TableRowGroup();
        table.RowGroups.Add(rg);

        var left = new TableCell
        {
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding = new Thickness(6)
        };

        left.Blocks.Add(ReportFormatting.MetaParagraph("RO #:", workOrder.WorkOrderNumber ?? string.Empty));
        left.Blocks.Add(ReportFormatting.MetaParagraph("DATE:", workOrder.CreatedAt.ToString("g")));
        left.Blocks.Add(ReportFormatting.MetaParagraph("CUSTOMER:", workOrder.Customer?.FullName ?? string.Empty));

        var right = new TableCell
        {
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding = new Thickness(6)
        };

        var vehicleText = $"{workOrder.Vehicle?.Year} {workOrder.Vehicle?.Make} {workOrder.Vehicle?.Model}".Trim();
        right.Blocks.Add(ReportFormatting.MetaParagraph("VEHICLE:", vehicleText));
        right.Blocks.Add(ReportFormatting.MetaParagraph("VIN:", workOrder.Vehicle?.Vin ?? string.Empty));
        right.Blocks.Add(ReportFormatting.MetaParagraph("LICENSE:", workOrder.Vehicle?.LicensePlate ?? string.Empty));
        right.Blocks.Add(ReportFormatting.MetaParagraph("MILEAGE IN:", workOrder.Vehicle?.Mileage?.ToString() ?? string.Empty));
        right.Blocks.Add(ReportFormatting.MetaParagraph("MILEAGE OUT:", workOrder.Vehicle?.MileageOut?.ToString() ?? string.Empty));

        rg.Rows.Add(new TableRow { Cells = { left, right } });
        doc.Blocks.Add(table);
    }

    private static void AddInspectionLegend(FlowDocument doc)
    {
        var table = new Table { CellSpacing = 0, Margin = new Thickness(0, 2, 0, 6) };
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

        var rg = new TableRowGroup();
        table.RowGroups.Add(rg);

        rg.Rows.Add(new TableRow
        {
            Cells =
            {
                ReportFormatting.LegendCell("GOOD", Brushes.Green, Brushes.White),
                ReportFormatting.LegendCell("FUTURE ATTENTION", Brushes.Goldenrod, Brushes.Black),
                ReportFormatting.LegendCell("NEEDS IMMEDIATE ATTENTION", Brushes.DarkRed, Brushes.White),
                ReportFormatting.LegendCell("NOT INSPECTED", Brushes.LightGray, Brushes.Black)
            }
        });

        doc.Blocks.Add(table);
    }

    private static void AddInspectionSections(FlowDocument doc, WorkOrder workOrder)
    {
        var items = workOrder.Inspection?.Items ?? Enumerable.Empty<WorkOrderInspectionItem>();

        foreach (var group in items
            .GroupBy(x => x.Section ?? string.Empty)
            .OrderBy(g => SectionOrder(g.Key)))
        {
            var table = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 6) };
            table.Columns.Add(new TableColumn { Width = new GridLength(2.6, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(2, GridUnitType.Star) });

            var rg = new TableRowGroup();
            table.RowGroups.Add(rg);

            var header = ReportFormatting.Cell(group.Key.ToUpperInvariant(), true, TextAlignment.Left, Brushes.Navy, Brushes.White);
            header.ColumnSpan = 3;
            rg.Rows.Add(new TableRow { Cells = { header } });

            foreach (var item in group.OrderBy(x => x.SortOrder).ThenBy(x => x.ItemName))
            {
                var statusBrush = GetInspectionBrush(item.Status);
                var statusText = GetInspectionStatusText(item.Status);
                var notes = item.Notes ?? string.Empty;

                rg.Rows.Add(new TableRow
                {
                    Cells =
                    {
                        ReportFormatting.Cell(item.ItemName ?? string.Empty),
                        ReportFormatting.Cell(statusText, true, TextAlignment.Center, statusBrush, GetContrastingForeground(statusBrush)),
                        ReportFormatting.Cell(notes)
                    }
                });
            }

            doc.Blocks.Add(table);
        }
    }
    private static void AddPageBreak(FlowDocument doc)
    {
        doc.Blocks.Add(new Paragraph
        {
            Margin = new Thickness(0),
            BreakPageBefore = true
        });
    }
    private static string BuildCustomerBlock(WorkOrder workOrder)
    {
        var customer = workOrder.Customer;

        var cityStateZip = string.Join(" ",
            new[] { customer?.City, customer?.State, customer?.PostalCode }
                .Where(x => !string.IsNullOrWhiteSpace(x)));

        return
            "CUSTOMER\n" +
            $"{customer?.FullName ?? string.Empty}\n" +
            $"{customer?.AddressLine1 ?? string.Empty}\n" +
            $"{customer?.AddressLine2 ?? string.Empty}\n" +
            $"{cityStateZip}\n" +
            $"{customer?.Phone ?? string.Empty}";
    }
    private static string BuildInvoiceBlock(WorkOrder workOrder, ShopSettings shop)
    {
        var advisor = AppSession.CurrentUser?.DisplayName ?? "UNASSIGNED";
        var tech = workOrder.Technician?.FullName ?? "UNASSIGNED";

        return
            $"RO #: {workOrder.WorkOrderNumber ?? string.Empty}\n" +
            $"DATE: {workOrder.CreatedAt:g}\n" +
            $"SERVICE ADVISOR: {advisor}\n" +
            $"TECHNICIAN: {tech}\n" +
            $"STATUS: {workOrder.StatusDisplay}";
    }
    private static TableCell BlockCell(string text)
    {
        var cell = new TableCell
        {
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding = new Thickness(1)
        };

        cell.Blocks.Add(new Paragraph { Margin = new Thickness(0), Inlines = { new Run(text ?? string.Empty) } });
        return cell;
    }
    private static Brush GetInspectionBrush(InspectionStatus status)
    {
        return status switch
        {
            InspectionStatus.Good => Brushes.Green,
            InspectionStatus.FutureAttention => Brushes.Goldenrod,
            InspectionStatus.ImmediateAttention => Brushes.DarkRed,
            _ => Brushes.LightGray
        };
    }
    private static Brush GetContrastingForeground(Brush brush)
    {
        if (brush == Brushes.Goldenrod || brush == Brushes.LightGray || brush == Brushes.Yellow)
            return Brushes.Black;

        return Brushes.White;
    }
    private static string GetInspectionStatusText(InspectionStatus status)
    {
        return status switch
        {
            InspectionStatus.Good => "GOOD",
            InspectionStatus.FutureAttention => "FUTURE ATTENTION",
            InspectionStatus.ImmediateAttention => "NEEDS IMMEDIATE ATTENTION",
            _ => "NOT INSPECTED"
        };
    }
    private static int SectionOrder(string section)
    {
        return section switch
        {
            "Exterior" => 1,
            "Tires / Brakes" => 2,
            "Under Hood" => 3,
            "Under Vehicle" => 4,
            _ => 99
        };
    }
    private static bool IsReceipt(WorkOrder? workOrder)
    {
        return workOrder?.Status is WorkOrderStatus.Completed or WorkOrderStatus.Paid or WorkOrderStatus.Closed;
    }
    private static TableRow LabelValueRow(string label, string value)
    {
        return new TableRow
        {
            Cells =
        {
            ReportFormatting.Cell(label, true),
            ReportFormatting.Cell(value, false, TextAlignment.Right)
        }
        };
    }
    private static void AppendInvoiceItemPage(FlowDocument doc,WorkOrder workOrder,ShopSettings shop,string docKind,string copyLabel,IReadOnlyList<WorkOrderLineItem> pageItems,int pageNumber,int totalPages,bool isFirstPage,bool isLastItemPage)
    {
        if (isFirstPage)
        {
            ReportFormatting.AddCompanyHeader(doc, shop, docKind, copyLabel);
            doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 2, 0, 2) });

            AddCustomerAndInvoiceInfo(doc, workOrder, shop);
            doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 2, 0, 2) });

            AddVehicleTable(doc, workOrder);
            doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 2, 0, 2) });
        }
        else
        {
            ReportFormatting.AddCentered(doc, $"{docKind} - CONTINUED", 12, true);
            ReportFormatting.AddCentered(doc, copyLabel, 10, true);
            doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 2, 0, 4) });
        }

        AddLineItemsTable(doc, pageItems);
        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 2, 0, 2) });

        if (isLastItemPage)
        {
            AddNotesAndTotalsSection(doc, workOrder);
            doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 2, 0, 2) });

            AddAuthorizationBlock(doc);
            doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 2, 0, 2) });

            ReportFormatting.AddCentered(doc, $"{docKind} • {copyLabel}", 11, true);
        }

        ReportFormatting.AddPageNumberFooter(doc, pageNumber, totalPages);
    }
    private static void AppendChecklistPage(FlowDocument doc,WorkOrder workOrder,ShopSettings shop,string copyLabel,int pageNumber,int totalPages)
    {
        ReportFormatting.AddCompanyHeader(doc, shop, "VEHICLE INSPECTION CHECKLIST", copyLabel);
        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 2, 0, 2) });

        AddInspectionMetaTable(doc, workOrder);
        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 2, 0, 2) });

        AddInspectionLegend(doc);
        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 2, 0, 2) });

        AddInspectionSections(doc, workOrder);
        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 2, 0, 2) });
        ReportFormatting.AddCentered(doc, copyLabel, 11, true);

        ReportFormatting.AddPageNumberFooter(doc, pageNumber, totalPages);
    }
    private static List<WorkOrderLineItem> GetOrderedLineItems(WorkOrder workOrder)
    {
        return (workOrder.LineItems ?? Enumerable.Empty<WorkOrderLineItem>())
            .OrderBy(x => x.ItemType == WorkOrderLineItemType.Labor ? 1 : 0)
            .ToList();
    }
    private static List<List<WorkOrderLineItem>> ChunkByVariable(List<WorkOrderLineItem> items,int firstPageSize,int continuationPageSize)
    {
        var result = new List<List<WorkOrderLineItem>>();

        var index = 0;

        if (items.Count > 0)
        {
            result.Add(items.Take(firstPageSize).ToList());
            index = firstPageSize;
        }

        while (index < items.Count)
        {
            result.Add(items
                .Skip(index)
                .Take(continuationPageSize)
                .ToList());

            index += continuationPageSize;
        }

        return result;
    }
}