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
using System.Windows;
using System.Windows.Media;
using System.Linq;

namespace AutoShop.MainApp.ViewModels;

public class WorkOrderViewModel : INotifyPropertyChanged, IRefreshable
{
    private readonly WorkOrderService _workOrderService = new();
    private readonly VehicleService _vehicleService = new();
    private string _mileageOutText = string.Empty;
    private int? MileageOutValue => int.TryParse(MileageOutText, out var value) ? value : null;

    public WorkOrderLineItem? SelectedLineItem
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            RemoveLineCommand.RaiseCanExecuteChanged();
        }
    }
    private readonly PartService _partService = new();
    private WorkOrder? _selectedWorkOrder;
    private WorkOrder _currentWorkOrder = new();
    private Customer? _selectedCustomer;
    private Vehicle? _selectedVehicle;
    private string _searchText = string.Empty;
    private Technician? _selectedTechnician;
    private readonly InventoryService _inventoryService = new();
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
            BalanceDue = workOrder.BalanceDue,
            MileageIn = workOrder.MileageIn,
            MileageOut = workOrder.MileageOut,
            InventoryApplied = workOrder.InventoryApplied
        };

        SelectedStatus = workOrder.Status;

        LineItems.Clear();
        foreach (var item in workOrder.LineItems ?? Enumerable.Empty<WorkOrderLineItem>())
        {
            LineItems.Add(new WorkOrderLineItem
            {
                Id = item.Id,
                WorkOrderId = item.WorkOrderId,
                PartId = item.PartId,
                PartNumber = item.PartNumber,
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
        SelectedTechnician = Technicians.FirstOrDefault(t => t.Id == workOrder.TechnicianId);

        MileageOutText = workOrder.MileageOut?.ToString() ?? string.Empty;
        RecalculateCurrentTotals();
    }
    private void NewWorkOrder()
    {
        SelectedWorkOrder = null;
        SelectedCustomer = null;
        SelectedVehicle = null;
        SelectedTechnician = null;

        CurrentWorkOrder = new WorkOrder
        {
            Status = WorkOrderStatus.Draft,
            CreatedAt = DateTime.Now,
            InventoryApplied = false
        };

        SelectedStatus = WorkOrderStatus.Draft;
        MileageOutText = string.Empty;
        LineItems.Clear();
        RecalculateCurrentTotals();
    }
    private void SaveWorkOrder()
    {
        ApplySelectionsToCurrentWorkOrder();

        if (CurrentWorkOrder.Status == WorkOrderStatus.Completed)
        {
            ApplyCompletedWorkflow();
        }

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
        var window = new AutoShop.MainApp.Views.PartLookupWindow();

        if (window.ShowDialog() != true)
            return;

        var part = window.SelectedPart;
        if (part == null)
            return;

        LineItems.Add(new WorkOrderLineItem
        {
            ItemType = WorkOrderLineItemType.Part,
            PartId = part.Id,
            PartNumber = part.PartNumber,
            Description = part.Description,
            Quantity = 1m,
            UnitPrice = part.SellPrice
        });
    }
    private void RemoveSelectedLine()
    {
        if (SelectedLineItem == null)
            return;

        LineItems.Remove(SelectedLineItem);
        SelectedLineItem = null;
        RecalculateCurrentTotals();
    }
    private decimal GetDefaultLaborRate()
    {
        using var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>());
        return db.ShopSettings.FirstOrDefault()?.DefaultLaborRate ?? 0m;
    }
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        CurrentWorkOrder.LaborTotal = LineItems
            .Where(x => x.ItemType == WorkOrderLineItemType.Labor)
            .Sum(x => x.LineTotal);

        CurrentWorkOrder.PartsTotal = LineItems
            .Where(x => x.ItemType == WorkOrderLineItemType.Part)
            .Sum(x => x.LineTotal);

        using var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>());
        var taxRate = db.ShopSettings.FirstOrDefault()?.TaxRate ?? 0m;

        CurrentWorkOrder.TaxTotal = CurrentWorkOrder.PartsTotal * (taxRate / 100m);

        CurrentWorkOrder.GrandTotal =
            CurrentWorkOrder.LaborTotal +
            CurrentWorkOrder.PartsTotal +
            CurrentWorkOrder.TaxTotal -
            CurrentWorkOrder.DiscountTotal;

        CurrentWorkOrder.BalanceDue = CurrentWorkOrder.GrandTotal - CurrentWorkOrder.AmountPaid;

        OnPropertyChanged(nameof(CurrentWorkOrder));
    }
    private void GenerateReceipt()
    {
        if (CurrentWorkOrder.Id == 0)
            return;

        var workOrder = _workOrderService.GetWorkOrderById(CurrentWorkOrder.Id);
        if (workOrder == null)
        {
            MessageBox.Show("Could not load the work order for printing.");
            return;
        }

        var receiptViewModel = new ReceiptViewModel();
        receiptViewModel.LoadReceipt(workOrder);

        if (receiptViewModel.PreviewDocument == null)
        {
            MessageBox.Show("The invoice document could not be generated.");
            return;
        }

        var receiptWindow = new AutoShop.MainApp.Views.ReceiptWindow(receiptViewModel);
        receiptWindow.ShowDialog();
    }
    private void OpenInspection()
    {
        if (!CanOpenInspection)
        {
            MessageBox.Show("Inspection checklist is only available when the work order is Open, In Progress, or Waiting Approval.");
            return;
        }

        if (CurrentWorkOrder.Id == 0)
        {
            MessageBox.Show("Please save the work order first.");
            return;
        }

        var window = new AutoShop.MainApp.Views.InspectionChecklistWindow(CurrentWorkOrder.Id);
        window.ShowDialog();

        var refreshed = _workOrderService.GetWorkOrderById(CurrentWorkOrder.Id);
        if (refreshed != null)
        {
            LoadSelectedWorkOrder(refreshed);
            LoadWorkOrders();
        }
    }
    private void SetStatus(WorkOrderStatus status)
    {
        SelectedStatus = status;
        CurrentWorkOrder.Status = status;

        if (status == WorkOrderStatus.Completed)
        {
            ApplyCompletedWorkflow();
        }

        OnPropertyChanged(nameof(CurrentWorkOrder));
        RefreshWorkflowCommands();
    }
    private void LoadTechnicians()
    {
        Technicians.Clear();

        foreach (var tech in _workOrderService.GetTechnicians())
        {
            Technicians.Add(tech);
        }
    }
    private void ApplyCompletedMileage()
    {
        if (SelectedVehicle == null)
            return;

        if (!MileageOutValue.HasValue)
            return;

        CurrentWorkOrder.MileageIn = SelectedVehicle.Mileage;
        CurrentWorkOrder.MileageOut = MileageOutValue;

        _vehicleService.UpdateMileageOut(SelectedVehicle.Id, MileageOutValue.Value);
    }
    private void ApplySelectionsToCurrentWorkOrder()
    {
        CurrentWorkOrder.Status = SelectedStatus;
        CurrentWorkOrder.MileageIn = SelectedVehicle?.Mileage;
        CurrentWorkOrder.MileageOut = MileageOutValue;
        CurrentWorkOrder.LineItems = LineItems.ToList();

        if (SelectedCustomer != null)
            CurrentWorkOrder.CustomerId = SelectedCustomer.Id;

        if (SelectedVehicle != null)
            CurrentWorkOrder.VehicleId = SelectedVehicle.Id;

        if (SelectedTechnician != null)
            CurrentWorkOrder.TechnicianId = SelectedTechnician.Id;
    }
    private void RefreshWorkflowCommands()
    {
        OpenInspectionCommand.RaiseCanExecuteChanged();
        SetOpenCommand.RaiseCanExecuteChanged();
        SetInProgressCommand.RaiseCanExecuteChanged();
        SetWaitingApprovalCommand.RaiseCanExecuteChanged();
        SetCompletedCommand.RaiseCanExecuteChanged();
        SetPaidCommand.RaiseCanExecuteChanged();
    }
    private void ApplyCompletedWorkflow()
    {
        CurrentWorkOrder.CompletedAt ??= DateTime.Now;
        CurrentWorkOrder.MileageIn = SelectedVehicle?.Mileage;
        CurrentWorkOrder.MileageOut = MileageOutValue;

        ApplyCompletedMileage();
        ApplyInventoryForCompletedWorkOrder();
    }
    private void ApplyInventoryForCompletedWorkOrder()
    {
        if (CurrentWorkOrder.InventoryApplied)
            return;

        foreach (var line in LineItems.Where(x =>
                     x.ItemType == WorkOrderLineItemType.Part &&
                     x.PartId.HasValue))
        {
            _inventoryService.AdjustQuantity(
                line.PartId.Value,
                -(int)line.Quantity,
                "Work Order",
                CurrentWorkOrder.WorkOrderNumber ?? string.Empty,
                "Work order completion");
        }

        CurrentWorkOrder.InventoryApplied = true;
    }
    public string MileageOutText
    {
        get => _mileageOutText;
        set
        {
            _mileageOutText = value;
            OnPropertyChanged();
        }
    }
    public ObservableCollection<WorkOrder> WorkOrders { get; } = new();
    public ObservableCollection<Customer> Customers { get; } = new();
    public ObservableCollection<Vehicle> Vehicles { get; } = new();
    public ObservableCollection<WorkOrderLineItem> LineItems { get; } = new();
    public string CurrentStatusDisplay => SelectedStatus switch
    {
        WorkOrderStatus.Draft => "DRAFT",
        WorkOrderStatus.Open => "OPEN",
        WorkOrderStatus.InProgress => "IN PROGRESS",
        WorkOrderStatus.WaitingApproval => "WAITING APPROVAL",
        WorkOrderStatus.Completed => "COMPLETED",
        WorkOrderStatus.Paid => "PAID",
        WorkOrderStatus.Closed => "CLOSED",
        WorkOrderStatus.Cancelled => "CANCELLED",
        _ => SelectedStatus.ToString().ToUpperInvariant()
    };
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
    public WorkOrder CurrentWorkOrder
    {
        get => _currentWorkOrder;
        set
        {
            _currentWorkOrder = value;
            OnPropertyChanged();
        }
    }
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
    public RelayCommand RemoveLineCommand { get; }
    public ICommand GenerateReceiptCommand { get; }
    public RelayCommand OpenInspectionCommand { get; }
    public RelayCommand SetOpenCommand { get; }
    public RelayCommand SetInProgressCommand { get; }
    public RelayCommand SetWaitingApprovalCommand { get; }
    public RelayCommand SetCompletedCommand { get; }
    public RelayCommand SetPaidCommand { get; }
    public event PropertyChangedEventHandler? PropertyChanged;
    private WorkOrderStatus _selectedStatus;
    public WorkOrderStatus SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            if (_selectedStatus == value)
                return;

            _selectedStatus = value;
            CurrentWorkOrder.Status = value;
            OnPropertyChanged(nameof(CurrentStatusDisplay));
            OnPropertyChanged(nameof(StatusBrush));
            OnPropertyChanged();
            RefreshWorkflowCommands();
        }
    }
    public WorkOrderViewModel()
    {
        NewCommand = new RelayCommand(NewWorkOrder);
        SaveCommand = new RelayCommand(SaveWorkOrder);
        DeleteCommand = new RelayCommand(DeleteWorkOrder);
        RefreshCommand = new RelayCommand(LoadWorkOrders);
        AddLaborLineCommand = new RelayCommand(AddLaborLine);
        AddPartLineCommand = new RelayCommand(AddPartLine);
        RemoveLineCommand = new RelayCommand(RemoveSelectedLine, () => SelectedLineItem != null);
        GenerateReceiptCommand = new RelayCommand(GenerateReceipt);
        LineItems.CollectionChanged += LineItems_CollectionChanged;
        OpenInspectionCommand = new RelayCommand(OpenInspection, () => CanOpenInspection);
        SetOpenCommand = new RelayCommand(() => SetStatus(WorkOrderStatus.Open), () => CanSetOpen);
        SetInProgressCommand = new RelayCommand(() => SetStatus(WorkOrderStatus.InProgress), () => CanSetInProgress);
        SetWaitingApprovalCommand = new RelayCommand(() => SetStatus(WorkOrderStatus.WaitingApproval), () => CanSetWaitingApproval);
        SetCompletedCommand = new RelayCommand(() => SetStatus(WorkOrderStatus.Completed), () => CanSetCompleted);
        SetPaidCommand = new RelayCommand(() => SetStatus(WorkOrderStatus.Paid), () => CanSetPaid);
        LoadCustomers();
        LoadVehicles();
        LoadTechnicians();
        LoadWorkOrders();
        NewWorkOrder();
    }
    public void Refresh()
    {
        LoadCustomers();
        LoadVehicles();
        LoadTechnicians();
        LoadWorkOrders();
    }   
    public Brush StatusBrush => SelectedStatus switch
    {
        WorkOrderStatus.Draft => Brushes.Gray,
        WorkOrderStatus.Open => Brushes.DodgerBlue,
        WorkOrderStatus.InProgress => Brushes.DarkOrange,
        WorkOrderStatus.WaitingApproval => Brushes.Goldenrod,
        WorkOrderStatus.Completed => Brushes.Green,
        WorkOrderStatus.Paid => Brushes.DarkGreen,
        WorkOrderStatus.Closed => Brushes.Black,
        WorkOrderStatus.Cancelled => Brushes.Red,
        _ => Brushes.Gray
    };
    public bool CanSetOpen => SelectedStatus == WorkOrderStatus.Draft;
    public bool CanSetInProgress => SelectedStatus == WorkOrderStatus.Open;
    public bool CanSetWaitingApproval => SelectedStatus == WorkOrderStatus.InProgress;
    public bool CanSetCompleted => SelectedStatus == WorkOrderStatus.InProgress || SelectedStatus == WorkOrderStatus.WaitingApproval;
    public bool CanSetPaid => SelectedStatus == WorkOrderStatus.Completed;
    public bool CanOpenInspection =>
        SelectedStatus == WorkOrderStatus.Open ||
        SelectedStatus == WorkOrderStatus.InProgress ||
        SelectedStatus == WorkOrderStatus.WaitingApproval;
    public ObservableCollection<Technician> Technicians { get; } = new();
    public Technician? SelectedTechnician
    {
        get => _selectedTechnician;
        set
        {
            _selectedTechnician = value;
            OnPropertyChanged();

            if (value != null)
            {
                CurrentWorkOrder.TechnicianId = value.Id;
            }
        }
    }
}