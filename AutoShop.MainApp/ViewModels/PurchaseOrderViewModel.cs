using AutoShop.Core.Entities;
using AutoShop.Core.Enums;
using AutoShop.MainApp.Helpers;
using AutoShop.MainApp.Views;
using AutoShop.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace AutoShop.MainApp.ViewModels;

public class PurchaseOrderViewModel : INotifyPropertyChanged, IRefreshable
{
    private readonly PurchaseOrderService _purchaseOrderService = new();

    public ObservableCollection<PurchaseOrder> PurchaseOrders { get; } = new();
    public ObservableCollection<PurchaseOrderLineItem> LineItems { get; } = new();

    private PurchaseOrder? _selectedPurchaseOrder;
    public PurchaseOrder? SelectedPurchaseOrder
    {
        get => _selectedPurchaseOrder;
        set
        {
            _selectedPurchaseOrder = value;
            OnPropertyChanged();

            if (value != null)
            {
                LoadSelectedPurchaseOrder(value);
            }
        }
    }

    private PurchaseOrder _currentPurchaseOrder = new();
    public PurchaseOrder CurrentPurchaseOrder
    {
        get => _currentPurchaseOrder;
        set
        {
            _currentPurchaseOrder = value;
            OnPropertyChanged();
        }
    }

    private PurchaseOrderLineItem? _selectedLineItem;
    public PurchaseOrderLineItem? SelectedLineItem
    {
        get => _selectedLineItem;
        set
        {
            _selectedLineItem = value;
            OnPropertyChanged();
            RemoveLineCommand.RaiseCanExecuteChanged();
        }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
        }
    }

    private string _supplierFilter = string.Empty;
    public string SupplierFilter
    {
        get => _supplierFilter;
        set
        {
            _supplierFilter = value;
            OnPropertyChanged();
        }
    }

    private decimal _orderedTotal;
    public decimal OrderedTotal
    {
        get => _orderedTotal;
        private set
        {
            _orderedTotal = value;
            OnPropertyChanged();
        }
    }

    private decimal _receivedTotal;
    public decimal ReceivedTotal
    {
        get => _receivedTotal;
        private set
        {
            _receivedTotal = value;
            OnPropertyChanged();
        }
    }

    private int _orderedQuantityTotal;
    public int OrderedQuantityTotal
    {
        get => _orderedQuantityTotal;
        private set
        {
            _orderedQuantityTotal = value;
            OnPropertyChanged();
        }
    }

    private int _receivedQuantityTotal;
    public int ReceivedQuantityTotal
    {
        get => _receivedQuantityTotal;
        private set
        {
            _receivedQuantityTotal = value;
            OnPropertyChanged();
        }
    }

    private int _remainingQuantityTotal;
    public int RemainingQuantityTotal
    {
        get => _remainingQuantityTotal;
        private set
        {
            _remainingQuantityTotal = value;
            OnPropertyChanged();
        }
    }

    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand CreateDraftCommand { get; }
    public ICommand MarkOrderedCommand { get; }
    public ICommand ReceiveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand AddPartCommand { get; }
    public RelayCommand RemoveLineCommand { get; }

    public PurchaseOrderViewModel()
    {
        NewCommand = new RelayCommand(NewPurchaseOrder);
        SaveCommand = new RelayCommand(SavePurchaseOrder);
        RefreshCommand = new RelayCommand(LoadPurchaseOrders);
        CreateDraftCommand = new RelayCommand(CreateDraftFromLowStock);
        MarkOrderedCommand = new RelayCommand(MarkOrdered);
        ReceiveCommand = new RelayCommand(ReceivePurchaseOrder);
        CancelCommand = new RelayCommand(CancelPurchaseOrder);
        AddPartCommand = new RelayCommand(AddPartLine);
        RemoveLineCommand = new RelayCommand(RemoveSelectedLine, () => SelectedLineItem != null);

        LineItems.CollectionChanged += LineItems_CollectionChanged;

        LoadPurchaseOrders();
        NewPurchaseOrder();
    }

    private void LoadPurchaseOrders()
    {
        var selectedId = SelectedPurchaseOrder?.Id;

        PurchaseOrders.Clear();

        foreach (var po in _purchaseOrderService.GetPurchaseOrders(SearchText))
        {
            PurchaseOrders.Add(po);
        }

        if (selectedId.HasValue)
        {
            SelectedPurchaseOrder = PurchaseOrders.FirstOrDefault(x => x.Id == selectedId.Value);
        }
    }

    private void LoadSelectedPurchaseOrder(PurchaseOrder purchaseOrder)
    {
        CurrentPurchaseOrder = ClonePurchaseOrder(purchaseOrder);

        LineItems.Clear();
        foreach (var item in purchaseOrder.LineItems.OrderBy(x => x.Id))
        {
            LineItems.Add(CloneLineItem(item));
        }

        RefreshTotals();
        OnPropertyChanged(nameof(CurrentPurchaseOrder));
    }

    private void NewPurchaseOrder()
    {
        SelectedPurchaseOrder = null;

        CurrentPurchaseOrder = new PurchaseOrder
        {
            Status = PurchaseOrderStatus.Draft,
            CreatedAt = DateTime.Now,
            Supplier = string.Empty,
            Notes = string.Empty
        };

        LineItems.Clear();
        SelectedLineItem = null;

        RefreshTotals();
    }

    private void SavePurchaseOrder()
    {
        SyncCurrentPurchaseOrder();

        var saved = _purchaseOrderService.SavePurchaseOrder(CurrentPurchaseOrder);
        LoadPurchaseOrders();
        SelectedPurchaseOrder = saved;
    }

    private void CreateDraftFromLowStock()
    {
        var draft = _purchaseOrderService.CreateDraftFromLowStock(SupplierFilter);

        if (draft == null)
        {
            MessageBox.Show("No low-stock parts were found.");
            return;
        }

        LoadPurchaseOrders();
        SelectedPurchaseOrder = draft;
    }

    private void MarkOrdered()
    {
        SyncCurrentPurchaseOrder();

        if (CurrentPurchaseOrder.Id == 0)
        {
            MessageBox.Show("Save the purchase order first.");
            return;
        }

        CurrentPurchaseOrder.Status = PurchaseOrderStatus.Ordered;
        _purchaseOrderService.SavePurchaseOrder(CurrentPurchaseOrder);
        _purchaseOrderService.MarkOrdered(CurrentPurchaseOrder.Id);

        LoadPurchaseOrders();
        SelectedPurchaseOrder = _purchaseOrderService.GetPurchaseOrderById(CurrentPurchaseOrder.Id);
    }

    private void ReceivePurchaseOrder()
    {
        SyncCurrentPurchaseOrder();

        if (CurrentPurchaseOrder.Id == 0)
        {
            MessageBox.Show("Save the purchase order first.");
            return;
        }

        if (!LineItems.Any(x => x.QuantityReceived > 0))
        {
            MessageBox.Show("Enter received quantities before receiving the purchase order.");
            return;
        }

        CurrentPurchaseOrder.Status = PurchaseOrderStatus.PartialReceived;
        var updated = _purchaseOrderService.ReceivePurchaseOrder(CurrentPurchaseOrder);

        LoadPurchaseOrders();
        SelectedPurchaseOrder = updated;
    }

    private void CancelPurchaseOrder()
    {
        if (CurrentPurchaseOrder.Id == 0)
        {
            MessageBox.Show("Save the purchase order first.");
            return;
        }

        CurrentPurchaseOrder.Status = PurchaseOrderStatus.Cancelled;
        _purchaseOrderService.CancelPurchaseOrder(CurrentPurchaseOrder.Id);

        LoadPurchaseOrders();
        SelectedPurchaseOrder = _purchaseOrderService.GetPurchaseOrderById(CurrentPurchaseOrder.Id);
    }

    private void AddPartLine()
    {
        var window = new PartLookupWindow();

        if (window.ShowDialog() != true)
            return;

        var part = window.SelectedPart;
        if (part == null)
            return;

        var existing = LineItems.FirstOrDefault(x => x.PartNumber == part.PartNumber);
        if (existing != null)
        {
            existing.QuantityOrdered += 1;
            existing.UnitCost = part.Cost;
        }
        else
        {
            LineItems.Add(new WorkOrderLineItem
            {
                PartId = part.Id,
                PartNumber = part.PartNumber,
                Description = part.Description,
                ItemType = WorkOrderLineItemType.Part,
                Quantity = 1,
                UnitPrice = part.SellPrice
            });
        }

        if (string.IsNullOrWhiteSpace(CurrentPurchaseOrder.Supplier) && !string.IsNullOrWhiteSpace(part.Supplier))
        {
            CurrentPurchaseOrder.Supplier = part.Supplier;
            OnPropertyChanged(nameof(CurrentPurchaseOrder));
        }

        RefreshTotals();
    }

    private void RemoveSelectedLine()
    {
        if (SelectedLineItem == null)
            return;

        LineItems.Remove(SelectedLineItem);
        SelectedLineItem = null;
        RefreshTotals();
    }

    public void RefreshTotals()
    {
        SyncCurrentPurchaseOrder();

        OrderedQuantityTotal = LineItems.Sum(x => x.QuantityOrdered);
        ReceivedQuantityTotal = LineItems.Sum(x => x.QuantityReceived);
        RemainingQuantityTotal = LineItems.Sum(x => Math.Max(x.QuantityOrdered - x.QuantityReceived, 0));
        OrderedTotal = LineItems.Sum(x => x.QuantityOrdered * x.UnitCost);
        ReceivedTotal = LineItems.Sum(x => x.QuantityReceived * x.UnitCost);

        OnPropertyChanged(nameof(CurrentPurchaseOrder));
    }

    private void SyncCurrentPurchaseOrder()
    {
        CurrentPurchaseOrder.LineItems = LineItems.Select(CloneLineItem).ToList();
    }

    private static PurchaseOrder ClonePurchaseOrder(PurchaseOrder source)
    {
        return new PurchaseOrder
        {
            Id = source.Id,
            PoNumber = source.PoNumber,
            Supplier = source.Supplier,
            Status = source.Status,
            CreatedAt = source.CreatedAt,
            OrderedAt = source.OrderedAt,
            ReceivedAt = source.ReceivedAt,
            Notes = source.Notes,
            LineItems = source.LineItems.Select(CloneLineItem).ToList()
        };
    }

    private static PurchaseOrderLineItem CloneLineItem(PurchaseOrderLineItem source)
    {
        return new PurchaseOrderLineItem
        {
            Id = source.Id,
            PurchaseOrderId = source.PurchaseOrderId,
            PartId = source.PartId,
            PartNumber = source.PartNumber,
            Description = source.Description,
            QuantityOrdered = source.QuantityOrdered,
            QuantityReceived = source.QuantityReceived,
            UnitCost = source.UnitCost
        };
    }

    private void LineItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshTotals();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Refresh()
    {
        LoadPurchaseOrders();
    }
}