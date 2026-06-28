using AutoShop.Core.Entities;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AutoShop.MainApp.Services;

public static class ReportFormatting
{
    public static FlowDocument CreateBaseDocument(
        double fontSize = 8.75,
        double pagePadding = 12,
        double pageWidth = 816,
        double pageHeight = 1056)
    {
        return new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = fontSize,
            PageWidth = pageWidth,
            PageHeight = pageHeight,
            PagePadding = new Thickness(pagePadding),
            ColumnWidth = double.PositiveInfinity
        };
    }

    public static void AddCompanyHeader(
        FlowDocument doc,
        ShopSettings shop,
        string title,
        string copyLabel,
        bool includeLogo = true)
    {
        var root = new Grid
        {
            Margin = new Thickness(0, 0, 0, 6)
        };

        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(165) });

        if (includeLogo)
        {
            var logo = TryBuildLogo(shop.LogoPath);
            if (logo != null)
            {
                Grid.SetColumn(logo, 0);
                root.Children.Add(logo);
            }
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
            Text = title,
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

    public static void AddCentered(FlowDocument doc, string text, double size = 10, bool bold = false)
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

    public static void AddPageNumberFooter(FlowDocument doc, int pageNumber, int totalPages)
    {
        doc.Blocks.Add(new Paragraph
        {
            Margin = new Thickness(6, 8, 6, 0),
            TextAlignment = TextAlignment.Center,
            Inlines =
            {
                new Run($"PAGE {pageNumber} OF {totalPages}")
                {
                    FontSize = 9,
                    FontWeight = FontWeights.Bold
                }
            }
        });
    }

    public static TextBlock HeaderLine(string text)
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

    public static Paragraph MetaParagraph(string label, string value)
    {
        var p = new Paragraph { Margin = new Thickness(0) };
        p.Inlines.Add(new Run(label) { FontWeight = FontWeights.Bold });
        p.Inlines.Add(new Run(" "));
        p.Inlines.Add(new Run(value ?? string.Empty));
        return p;
    }

    public static TableCell Cell(
        string text,
        bool bold = false,
        TextAlignment align = TextAlignment.Left,
        Brush? background = null,
        Brush? foreground = null,
        double padding = 3)
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
            Padding = new Thickness(padding)
        };

        if (background != null)
            cell.Background = background;

        return cell;
    }

    public static TableCell LegendCell(string text, Brush background, Brush foreground)
    {
        return Cell(text, true, TextAlignment.Center, background, foreground);
    }

    public static Image? TryBuildLogo(string? logoPath)
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
}