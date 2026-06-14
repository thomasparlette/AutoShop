using AutoShop.MainApp.ViewModels;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;
namespace AutoShop.MainApp.Views;

public partial class ReceiptWindow : Window
{
    public ReceiptWindow(ReceiptViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Print_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ReceiptViewModel vm)
            return;

        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() != true)
            return;

        var document = new FlowDocument
        {
            FontFamily = new FontFamily("Consolas"),
            FontSize = 12,
            PagePadding = new Thickness(40),
            ColumnWidth = double.PositiveInfinity
        };

        var paragraph = new Paragraph
        {
            Margin = new Thickness(0)
        };

        foreach (var line in vm.ReceiptText.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
        {
            paragraph.Inlines.Add(new Run(line));
            paragraph.Inlines.Add(new LineBreak());
        }

        document.Blocks.Add(paragraph);

        document.PageWidth = printDialog.PrintableAreaWidth;
        document.PageHeight = printDialog.PrintableAreaHeight;

        IDocumentPaginatorSource idocument = document;
        printDialog.PrintDocument(idocument.DocumentPaginator, "AutoShop Receipt");
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}