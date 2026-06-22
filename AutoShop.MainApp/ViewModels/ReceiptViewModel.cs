using AutoShop.MainApp.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Documents;

namespace AutoShop.MainApp.ViewModels;

public class ReceiptViewModel : INotifyPropertyChanged
{
    private readonly InvoicePrintService _invoicePrintService = new();

    private FlowDocument? _previewDocument;
    public FlowDocument? PreviewDocument
    {
        get => _previewDocument;
        set
        {
            _previewDocument = value;
            OnPropertyChanged();
        }
    }

    private string _documentTitle = "Invoice Preview";
    public string DocumentTitle
    {
        get => _documentTitle;
        set
        {
            _documentTitle = value;
            OnPropertyChanged();
        }
    }

    public void LoadReceipt(AutoShop.Core.Entities.WorkOrder workOrder)
    {
        PreviewDocument = _invoicePrintService.BuildInvoiceDocument(workOrder);
        DocumentTitle = _invoicePrintService.GetDocumentTitle(workOrder);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}