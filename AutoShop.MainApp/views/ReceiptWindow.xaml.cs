using AutoShop.MainApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

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
        if (DataContext is not ReceiptViewModel vm || vm.PreviewDocument == null)
            return;

        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() != true)
            return;

        var paginator = ((IDocumentPaginatorSource)vm.PreviewDocument).DocumentPaginator;
        printDialog.PrintDocument(paginator, vm.DocumentTitle);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}