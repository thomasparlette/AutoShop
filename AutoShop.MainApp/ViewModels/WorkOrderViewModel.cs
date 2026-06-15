using AutoShop.Core.Entities;
using AutoShop.Core.Enums;
using AutoShop.Data;
using AutoShop.MainApp.Helpers;
using AutoShop.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AutoShop.MainApp.ViewModels;

public class WorkOrderViewModel : INotifyPropertyChanged, IRefreshable
{
    private readonly WorkOrderService _workOrderService = new();

    public ObservableCollection<WorkOrder> WorkOrders { get; } = new();
    public ObservableCollection<Customer> Customers { get; } = new();
    public ObservableCollection<Vehicle> Vehicles { get; } = new();
    public ObservableCollection<WorkOrderLineItem> LineItems { get; } = new();

    private WorkOrder? _selectedWorkOrder;
    public WorkOrder? SelectedWorkOrder
    {
        get => _selectedWorkOrder;
        set
        {
            _selectedWorkOrder = value;
            OnPropertyChanged();

            if (value != null)
            {
                LoadSelectedWorkOrder(value);
            }
        }
    }

    private WorkOrder _currentWorkOrder = new();
    public WorkOrder CurrentWorkOrder
    {
        get => _currentWorkOrder;
        set
        {
            _currentWorkOrder = value;
            OnPropertyChanged();
        }
    }

    private Customer? _selectedCustomer;
    public Customer? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            _selectedCustomer = value;
            OnPropertyChanged();

            if (value != null)
            {
                CurrentWorkOrder.CustomerId = value.Id;
                LoadVehiclesForCustomer(value.Id);
            }
        }
    }

    private Vehicle? _selectedVehicle;
    public Vehicle? SelectedVehicle
    {
        get => _selectedVehicle;
        set
        {
            _selectedVehicle = value;
            OnPropertyChanged();

            if (value != null)
            {
                CurrentWorkOrder.VehicleId = value.Id;
            }
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

    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand AddLaborLineCommand { get; }
    public ICommand AddPartLineCommand { get; }
    public ICommand RemoveLineCommand { get; }
    public ICommand GenerateReceiptCommand { get; }

    public WorkOrderViewModel()
    {
        NewCommand = new RelayCommand(NewWorkOrder);
        SaveCommand = new RelayCommand(SaveWorkOrder);
        DeleteCommand = new RelayCommand(DeleteWorkOrder);
        RefreshCommand = new RelayCommand(LoadWorkOrders);
        AddLaborLineCommand = new RelayCommand(AddLaborLine);
        AddPartLineCommand = new RelayCommand(AddPartLine);
        RemoveLineCommand = new RelayCommand(RemoveSelectedLine);
        GenerateReceiptCommand = new RelayCommand(GenerateReceipt);

        LineItems.CollectionChanged += LineItems_CollectionChanged;

        LoadCustomers();
        LoadVehicles();
        LoadWorkOrders();
        NewWorkOrder();
    }

    private void LoadCustomers()
    {
        Customers.Clear();

        foreach (var customer in _workOrderService.GetCustomers())
        {
            Customers.Add(customer);
        }
    }

    private void LoadVehicles()
    {
        Vehicles.Clear();

        foreach (var vehicle in _workOrderService.GetVehicles())
        {
            Vehicles.Add(vehicle);
        }
    }

    private void LoadVehiclesForCustomer(int customerId)
    {
        Vehicles.Clear();

        foreach (var vehicle in _workOrderService.GetVehicles().Where(v => v.CustomerId == customerId))
        {
            Vehicles.Add(vehicle);
        }
    }

    private void LoadWorkOrders()
    {
        WorkOrders.Clear();

        foreach (var workOrder in _workOrderService.GetWorkOrders(SearchText))
        {
            WorkOrders.Add(workOrder);
        }
    }

    private void LoadSelectedWorkOrder(WorkOrder workOrder)
    {
        CurrentWorkOrder = new WorkOrder
        {
            Id = workOrder.Id,
            WorkOrderNumber = workOrder.WorkOrderNumber,
            CustomerId = workOrder.CustomerId,
            VehicleId = workOrder.VehicleId,
            CreatedAt = workOrder.CreatedAt,
            CompletedAt = workOrder.CompletedAt,
            Status = workOrder.Status,
            Complaint = workOrder.Complaint,
            Diagnosis = workOrder.Diagnosis,
            Notes = workOrder.Notes,
            LaborTotal = workOrder.LaborTotal,
            PartsTotal = workOrder.PartsTotal,
            TaxTotal = workOrder.TaxTotal,
            DiscountTotal = workOrder.DiscountTotal,
            GrandTotal = workOrder.GrandTotal,
            AmountPaid = workOrder.AmountPaid,
            BalanceDue = workOrder.BalanceDue
        };

        LineItems.Clear();

        foreach (var item in workOrder.LineItems)
        {
            LineItems.Add(new WorkOrderLineItem
            {
                Id = item.Id,
                WorkOrderId = item.WorkOrderId,
                Description = item.Description,
                ItemType = item.ItemType,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal
            });
        }

        SelectedCustomer = Customers.FirstOrDefault(c => c.Id == workOrder.CustomerId);
        if (SelectedCustomer != null)
        {
            LoadVehiclesForCustomer(SelectedCustomer.Id);
        }

        SelectedVehicle = Vehicles.FirstOrDefault(v => v.Id == workOrder.VehicleId);

        RecalculateCurrentTotals();
    }

    private void NewWorkOrder()
    {
        SelectedWorkOrder = null;
        SelectedCustomer = null;
        SelectedVehicle = null;

        CurrentWorkOrder = new WorkOrder
        {
            Status = WorkOrderStatus.Draft,
            CreatedAt = DateTime.Now
        };

        LineItems.Clear();
        RecalculateCurrentTotals();
    }

    private void SaveWorkOrder()
    {
        CurrentWorkOrder.LineItems = LineItems.ToList();

        if (SelectedCustomer != null)
            CurrentWorkOrder.CustomerId = SelectedCustomer.Id;

        if (SelectedVehicle != null)
            CurrentWorkOrder.VehicleId = SelectedVehicle.Id;

        RecalculateCurrentTotals();

        _workOrderService.SaveWorkOrder(CurrentWorkOrder);
        LoadWorkOrders();
        NewWorkOrder();
    }

    private void DeleteWorkOrder()
    {
        if (CurrentWorkOrder.Id == 0)
            return;

        _workOrderService.DeleteWorkOrder(CurrentWorkOrder.Id);
        LoadWorkOrders();
        NewWorkOrder();
    }

    private void AddLaborLine()
    {
        LineItems.Add(new WorkOrderLineItem
        {
            ItemType = WorkOrderLineItemType.Labor,
            Description = "Labor",
            Quantity = 1m,
            UnitPrice = GetDefaultLaborRate()
        });
    }

    private void AddPartLine()
    {
        LineItems.Add(new WorkOrderLineItem
        {
            ItemType = WorkOrderLineItemType.Part,
            Description = "Part",
            Quantity = 1m,
            UnitPrice = 0m
        });
    }

    private void RemoveSelectedLine()
    {
        if (LineItems.Count > 0)
        {
            LineItems.RemoveAt(LineItems.Count - 1);
        }
    }

    private decimal GetDefaultLaborRate()
    {
        using var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>());
        return db.ShopSettings.FirstOrDefault()?.DefaultLaborRate ?? 0m;
    }

    public Array StatusOptions => Enum.GetValues(typeof(WorkOrderStatus));

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Refresh()
    {
        LoadCustomers();
        LoadVehicles();
        LoadWorkOrders();
    }

    private void LineItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (WorkOrderLineItem item in e.NewItems)
            {
                item.PropertyChanged += LineItem_PropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (WorkOrderLineItem item in e.OldItems)
            {
                item.PropertyChanged -= LineItem_PropertyChanged;
            }
        }

        RecalculateCurrentTotals();
    }

    private void LineItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        RecalculateCurrentTotals();
    }

    private void RecalculateCurrentTotals()
    {
        CurrentWorkOrder.LaborTotal = LineItems.Where(x => x.ItemType == WorkOrderLineItemType.Labor).Sum(x => x.LineTotal);
        CurrentWorkOrder.PartsTotal = LineItems.Where(x => x.ItemType == WorkOrderLineItemType.Part).Sum(x => x.LineTotal);
        CurrentWorkOrder.GrandTotal = CurrentWorkOrder.LaborTotal + CurrentWorkOrder.PartsTotal - CurrentWorkOrder.DiscountTotal + CurrentWorkOrder.TaxTotal;
        CurrentWorkOrder.BalanceDue = CurrentWorkOrder.GrandTotal - CurrentWorkOrder.AmountPaid;

        OnPropertyChanged(nameof(CurrentWorkOrder));
    }

    private void GenerateReceipt()
    {
        if (CurrentWorkOrder.Id == 0)
            return;

        var workOrder = _workOrderService.GetWorkOrderById(CurrentWorkOrder.Id);
        if (workOrder == null)
            return;

        var receiptViewModel = new ReceiptViewModel();
        receiptViewModel.LoadReceipt(workOrder);

        var receiptWindow = new AutoShop.MainApp.Views.ReceiptWindow(receiptViewModel);
        receiptWindow.ShowDialog();
    }
}