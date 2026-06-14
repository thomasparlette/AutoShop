using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoShop.Core.Entities;

public class WorkOrderLineItem : INotifyPropertyChanged
{
    private string _description = string.Empty;
    private decimal _laborHours;
    private decimal _laborRate;
    private decimal _partsCost;
    private decimal _lineTotal;
    private bool _isPart;

    public int Id { get; set; }

    public int WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged();
        }
    }

    public decimal LaborHours
    {
        get => _laborHours;
        set
        {
            _laborHours = value;
            Recalculate();
            OnPropertyChanged();
        }
    }

    public decimal LaborRate
    {
        get => _laborRate;
        set
        {
            _laborRate = value;
            Recalculate();
            OnPropertyChanged();
        }
    }

    public decimal PartsCost
    {
        get => _partsCost;
        set
        {
            _partsCost = value;
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

    public bool IsPart
    {
        get => _isPart;
        set
        {
            _isPart = value;
            Recalculate();
            OnPropertyChanged();
        }
    }

    private void Recalculate()
    {
        if (IsPart)
        {
            LineTotal = PartsCost;
        }
        else
        {
            LineTotal = LaborHours * LaborRate;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}