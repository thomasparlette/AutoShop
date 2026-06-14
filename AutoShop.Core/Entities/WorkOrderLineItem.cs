using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoShop.Core.Entities;

public enum WorkOrderLineItemType
{
    Labor = 0,
    Part = 1
}

public class WorkOrderLineItem : INotifyPropertyChanged
{
    private WorkOrderLineItemType _itemType;
    private string _description = string.Empty;
    private decimal _quantity = 1m;
    private decimal _unitPrice;
    private decimal _lineTotal;

    public int Id { get; set; }

    public int WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public WorkOrderLineItemType ItemType
    {
        get => _itemType;
        set
        {
            _itemType = value;
            Recalculate();
            OnPropertyChanged();
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged();
        }
    }

    public decimal Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            Recalculate();
            OnPropertyChanged();
        }
    }

    public decimal UnitPrice
    {
        get => _unitPrice;
        set
        {
            _unitPrice = value;
            Recalculate();
            OnPropertyChanged();
        }
    }

    public decimal LineTotal
    {
        get => _lineTotal;
        set
        {
            _lineTotal = value;
            OnPropertyChanged();
        }
    }

    private void Recalculate()
    {
        LineTotal = Quantity * UnitPrice;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}