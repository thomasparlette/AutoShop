using AutoShop.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoShop.MainApp.ViewModels;

public class ReceiptViewModel : INotifyPropertyChanged
{
    private readonly ReceiptService _receiptService = new();

    private string _receiptText = string.Empty;
    public string ReceiptText
    {
        get => _receiptText;
        set
        {
            _receiptText = value;
            OnPropertyChanged();
        }
    }

    public void LoadReceipt(AutoShop.Core.Entities.WorkOrder workOrder)
    {
        ReceiptText = _receiptService.BuildReceiptText(workOrder);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}