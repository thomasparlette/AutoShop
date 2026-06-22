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

        var doc = CreateBaseDocument();

        AppendInvoiceCopy(doc, safeWorkOrder, shop, internalCopy: true);
        AddPageBreak(doc);
        AppendChecklistCopy(doc, safeWorkOrder, shop, internalCopy: true);

        AddPageBreak(doc);
        AppendInvoiceCopy(doc, safeWorkOrder, shop, internalCopy: false);
        AddPageBreak(doc);
        AppendChecklistCopy(doc, safeWorkOrder, shop, internalCopy: false);

        return doc;
    }

    private static FlowDocument CreateBaseDocument()
    {
        return new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 10.5,
            PageWidth = 816,
            PageHeight = 1056,
            PagePadding = new Thickness(20),
            ColumnWidth = double.PositiveInfinity
        };
    }

    private static void AppendInvoiceCopy(FlowDocument doc, WorkOrder workOrder, ShopSettings shop, bool internalCopy)
    {
        var docKind = IsReceipt(workOrder) ? "RECEIPT" : "QUOTATION";
        var copyLabel = internalCopy ? "INTERNAL COPY" : "CUSTOMER COPY";

        AddHeader(doc, shop, docKind, copyLabel);
        AddCentered(doc, docKind, 14, true);
        AddCentered(doc, copyLabel, 11, true);
        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 4, 0, 4) });

        AddCustomerAndInvoiceInfo(doc, workOrder, shop);
        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 4, 0, 4) });

        AddVehicleTable(doc, workOrder);
        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 4, 0, 4) });

        AddLineItemsTable(doc, workOrder);
        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 4, 0, 4) });

        AddNotesAndTotalsSection(doc, workOrder);
        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 6, 0, 6) });

        AddAuthorizationBlock(doc);
        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 6, 0, 6) });

        AddCentered(doc, $"{docKind} • {copyLabel}", 11, true);
    }

    private static void AppendChecklistCopy(FlowDocument doc, WorkOrder workOrder, ShopSettings shop, bool internalCopy)
    {
        var copyLabel = internalCopy ? "INTERNAL COPY" : "CUSTOMER COPY";

        AddHeader(doc, shop, "VEHICLE INSPECTION", copyLabel);
        AddCentered(doc, "VEHICLE INSPECTION", 14, true);
        AddCentered(doc, copyLabel, 11, true);
        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 4, 0, 4) });

        AddInspectionMetaTable(doc, workOrder);
        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 4, 0, 4) });

        AddInspectionLegend(doc);
        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 4, 0, 4) });

        AddInspectionSections(doc, workOrder);
        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 6, 0, 6) });

        AddInspectionComments(doc, workOrder);
        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 8, 0, 6) });

        AddCentered(doc, copyLabel, 11, true);
    }

    private static void AddHeader(FlowDocument doc, ShopSettings shop, string docKind, string copyLabel)
    {
        var root = new Grid
        {
            Margin = new Thickness(0, 0, 0, 6)
        };

        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(165) });

        var logo = TryBuildLogo(shop.LogoPath);
        if (logo != null)
        {
            Grid.SetColumn(logo, 0);
            root.Children.Add(logo);
        }

        var infoPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        infoPanel.Children.Add(new TextBlock
        {
            Text = shop.ShopName ?? "AutoShop",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 2),
            TextWrapping = TextWrapping.Wrap
        });

        if (!string.IsNullOrWhiteSpace(shop.AddressLine1))
            infoPanel.Children.Add(HeaderLine(shop.AddressLine1));

        if (!string.IsNullOrWhiteSpace(shop.AddressLine2))
            infoPanel.Children.Add(HeaderLine(shop.AddressLine2));

        var cityStateZip = string.Join(" ",
            new[] { shop.City, shop.State, shop.PostalCode }
                .Where(x => !string.IsNullOrWhiteSpace(x)));

        if (!string.IsNullOrWhiteSpace(cityStateZip))
            infoPanel.Children.Add(HeaderLine(cityStateZip));

        if (!string.IsNullOrWhiteSpace(shop.Phone))
            infoPanel.Children.Add(HeaderLine(shop.Phone));

        if (!string.IsNullOrWhiteSpace(shop.Email))
            infoPanel.Children.Add(HeaderLine(shop.Email));

        if (!string.IsNullOrWhiteSpace(shop.Website))
            infoPanel.Children.Add(HeaderLine(shop.Website));

        Grid.SetColumn(infoPanel, 1);
        root.Children.Add(infoPanel);

        var rightPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        rightPanel.Children.Add(new TextBlock
        {
            Text = docKind,
            FontSize = 15,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.DarkRed,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(0, 0, 0, 2)
        });

        rightPanel.Children.Add(new TextBlock
        {
            Text = copyLabel,
            FontSize = 10.5,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Right
        });

        Grid.SetColumn(rightPanel, 2);
        root.Children.Add(rightPanel);

        doc.Blocks.Add(new BlockUIContainer(root));
    }

    private static TextBlock HeaderLine(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 10,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0),
            TextWrapping = TextWrapping.Wrap
        };
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
        table.Columns.Add(new TableColumn { Width = new GridLength(60) });
        table.Columns.Add(new TableColumn { Width = new GridLength(120) });
        table.Columns.Add(new TableColumn { Width = new GridLength(120) });
        table.Columns.Add(new TableColumn { Width = new GridLength(185) });
        table.Columns.Add(new TableColumn { Width = new GridLength(100) });
        table.Columns.Add(new TableColumn { Width = new GridLength(100) });

        var rg = new TableRowGroup();
        table.RowGroups.Add(rg);

        rg.Rows.Add(new TableRow
        {
            Cells =
            {
                Cell("YEAR", true),
                Cell("MAKE", true),
                Cell("MODEL", true),
                Cell("VIN", true),
                Cell("LICENSE", true),
                Cell("MILEAGE", true)
            }
        });

        rg.Rows.Add(new TableRow
        {
            Cells =
            {
                Cell(workOrder.Vehicle?.Year?.ToString() ?? string.Empty),
                Cell(workOrder.Vehicle?.Make ?? string.Empty),
                Cell(workOrder.Vehicle?.Model ?? string.Empty),
                Cell(workOrder.Vehicle?.Vin ?? string.Empty),
                Cell(workOrder.Vehicle?.LicensePlate ?? string.Empty),
                Cell(workOrder.Vehicle?.Mileage?.ToString() ?? string.Empty)
            }
        });

        doc.Blocks.Add(table);
    }

    private static void AddLineItemsTable(FlowDocument doc, WorkOrder workOrder)
    {
        var table = new Table { CellSpacing = 0 };
        table.Columns.Add(new TableColumn { Width = new GridLength(70) });
        table.Columns.Add(new TableColumn { Width = new GridLength(330) });
        table.Columns.Add(new TableColumn { Width = new GridLength(55) });
        table.Columns.Add(new TableColumn { Width = new GridLength(90) });
        table.Columns.Add(new TableColumn { Width = new GridLength(90) });

        var rg = new TableRowGroup();
        table.RowGroups.Add(rg);

        rg.Rows.Add(new TableRow
        {
            Cells =
            {
                Cell("TYPE", true),
                Cell("DESCRIPTION", true),
                Cell("QTY", true),
                Cell("UNIT", true),
                Cell("TOTAL", true)
            }
        });

        foreach (var item in workOrder.LineItems ?? Enumerable.Empty<WorkOrderLineItem>())
        {
            rg.Rows.Add(new TableRow
            {
                Cells =
                {
                    Cell(item.ItemType.ToString().ToUpperInvariant()),
                    Cell(item.Description ?? string.Empty),
                    Cell(item.Quantity.ToString("N2")),
                    Cell(item.UnitPrice.ToString("C")),
                    Cell(item.LineTotal.ToString("C"))
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

        left.Blocks.Add(MetaParagraph("RO #:", workOrder.WorkOrderNumber ?? string.Empty));
        left.Blocks.Add(MetaParagraph("DATE:", workOrder.CreatedAt.ToString("g")));
        left.Blocks.Add(MetaParagraph("CUSTOMER:", workOrder.Customer?.FullName ?? string.Empty));

        var right = new TableCell
        {
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding = new Thickness(6)
        };

        var vehicleText = $"{workOrder.Vehicle?.Year} {workOrder.Vehicle?.Make} {workOrder.Vehicle?.Model}".Trim();
        right.Blocks.Add(MetaParagraph("VEHICLE:", vehicleText));
        right.Blocks.Add(MetaParagraph("VIN:", workOrder.Vehicle?.Vin ?? string.Empty));
        right.Blocks.Add(MetaParagraph("LICENSE:", workOrder.Vehicle?.LicensePlate ?? string.Empty));
        right.Blocks.Add(MetaParagraph("MILEAGE:", workOrder.Vehicle?.Mileage?.ToString() ?? string.Empty));

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
                LegendCell("GOOD", Brushes.Green, Brushes.White),
                LegendCell("FUTURE ATTENTION", Brushes.Goldenrod, Brushes.Black),
                LegendCell("NEEDS IMMEDIATE ATTENTION", Brushes.DarkRed, Brushes.White),
                LegendCell("NOT INSPECTED", Brushes.LightGray, Brushes.Black)
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

            var header = Cell(group.Key.ToUpperInvariant(), true, TextAlignment.Left, Brushes.Navy, Brushes.White);
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
                        Cell(item.ItemName ?? string.Empty),
                        Cell(statusText, true, TextAlignment.Center, statusBrush, GetContrastingForeground(statusBrush)),
                        Cell(notes)
                    }
                });
            }

            doc.Blocks.Add(table);
        }
    }

    private static void AddInspectionComments(FlowDocument doc, WorkOrder workOrder)
    {
        doc.Blocks.Add(new Paragraph
        {
            Margin = new Thickness(0),
            FontWeight = FontWeights.Bold,
            Inlines = { new Run("COMMENTS / RECOMMENDATIONS") }
        });

        if (!string.IsNullOrWhiteSpace(workOrder.Inspection?.OverallNotes))
        {
            doc.Blocks.Add(new Paragraph
            {
                Margin = new Thickness(0),
                Inlines = { new Run(workOrder.Inspection.OverallNotes) }
            });
        }
        else
        {
            for (int i = 0; i < 5; i++)
                doc.Blocks.Add(new Paragraph { Margin = new Thickness(0) });
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
            Padding = new Thickness(6)
        };

        cell.Blocks.Add(new Paragraph { Margin = new Thickness(0), Inlines = { new Run(text ?? string.Empty) } });
        return cell;
    }

    private static TableCell Cell(
        string text,
        bool bold = false,
        TextAlignment align = TextAlignment.Left,
        Brush? background = null,
        Brush? foreground = null)
    {
        var p = new Paragraph
        {
            Margin = new Thickness(0),
            TextAlignment = align
        };

        p.Inlines.Add(new Run(text ?? string.Empty)
        {
            FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
            Foreground = foreground ?? Brushes.Black
        });

        var cell = new TableCell(p)
        {
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding = new Thickness(3)
        };

        if (background != null)
            cell.Background = background;

        return cell;
    }

    private static TableCell LegendCell(string text, Brush background, Brush foreground)
    {
        return Cell(text, true, TextAlignment.Center, background, foreground);
    }

    private static Image? TryBuildLogo(string? logoPath)
    {
        if (string.IsNullOrWhiteSpace(logoPath) || !File.Exists(logoPath))
            return null;

        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(logoPath, UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.DecodePixelWidth = 100;
            bmp.EndInit();
            bmp.Freeze();

            return new Image
            {
                Source = bmp,
                Width = 80,
                Height = 80,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Left
            };
        }
        catch
        {
            return null;
        }
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

    private static void AddCentered(FlowDocument doc, string text, double size = 10, bool bold = false)
    {
        var p = new Paragraph
        {
            Margin = new Thickness(0),
            TextAlignment = TextAlignment.Center
        };

        p.Inlines.Add(new Run(text ?? string.Empty)
        {
            FontSize = size,
            FontWeight = bold ? FontWeights.Bold : FontWeights.Normal
        });

        doc.Blocks.Add(p);
    }

    private static void AddSectionTitle(FlowDocument doc, string title)
    {
        var p = new Paragraph { Margin = new Thickness(0, 6, 0, 2) };
        p.Inlines.Add(new Run(title)
        {
            FontWeight = FontWeights.Bold
        });
        doc.Blocks.Add(p);
    }

    private static void AddParagraph(FlowDocument doc, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var p = new Paragraph { Margin = new Thickness(0) };
        p.Inlines.Add(new Run(text));
        doc.Blocks.Add(p);
    }

    private static Paragraph CenterParagraph(string text)
    {
        return new Paragraph
        {
            Margin = new Thickness(0),
            TextAlignment = TextAlignment.Center,
            Inlines = { new Run(text) }
        };
    }

    private static Paragraph MetaParagraph(string label, string value)
    {
        var p = new Paragraph { Margin = new Thickness(0) };
        p.Inlines.Add(new Run(label) { FontWeight = FontWeights.Bold });
        p.Inlines.Add(new Run(" "));
        p.Inlines.Add(new Run(value));
        return p;
    }

    private static TableRow LabelValueRow(string label, string value)
    {
        return new TableRow
        {
            Cells =
        {
            Cell(label, true),
            Cell(value, false, TextAlignment.Right)
        }
        };
    }
}