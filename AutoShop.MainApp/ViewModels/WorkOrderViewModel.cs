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
namespace AutoShop.MainApp.ViewModels;

public class WorkOrderViewModel : INotifyPropertyChanged, IRefreshable
{
    private readonly WorkOrderService _workOrderService = new();

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
    public RelayCommand OpenInspectionCommand { get; }
    public RelayCommand SetOpenCommand { get; }
    public RelayCommand SetInProgressCommand { get; }
    public RelayCommand SetWaitingApprovalCommand { get; }
    public RelayCommand SetCompletedCommand { get; }
    public RelayCommand SetPaidCommand { get; }
    public Array StatusOptions => Enum.GetValues(typeof(WorkOrderStatus));

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
        RemoveLineCommand = new RelayCommand(RemoveSelectedLine);
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

        SelectedStatus = workOrder.Status;

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
        SelectedTechnician = Technicians.FirstOrDefault(t => t.Id == workOrder.TechnicianId);
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

        SelectedStatus = WorkOrderStatus.Draft;
        SelectedTechnician = null;

        LineItems.Clear();
        RecalculateCurrentTotals();
    }

    private void SaveWorkOrder()
    {
        CurrentWorkOrder.Status = SelectedStatus;
        CurrentWorkOrder.LineItems = LineItems.ToList();

        if (SelectedCustomer != null)
            CurrentWorkOrder.CustomerId = SelectedCustomer.Id;

        if (SelectedVehicle != null)
            CurrentWorkOrder.VehicleId = SelectedVehicle.Id;
        
        if (SelectedTechnician != null)
            CurrentWorkOrder.TechnicianId = SelectedTechnician.Id;

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
        OnPropertyChanged(nameof(CurrentWorkOrder));
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
    public bool CanOpen =>
    SelectedStatus == WorkOrderStatus.Draft;

    public bool CanStartWork =>
        SelectedStatus == WorkOrderStatus.Open;

    public bool CanRequestApproval =>
        SelectedStatus == WorkOrderStatus.InProgress;

    public bool CanComplete =>
        SelectedStatus == WorkOrderStatus.InProgress ||
        SelectedStatus == WorkOrderStatus.WaitingApproval;

    public bool CanMarkPaid =>
        SelectedStatus == WorkOrderStatus.Completed;

    public bool CanSetOpen => SelectedStatus == WorkOrderStatus.Draft;
    public bool CanSetInProgress => SelectedStatus == WorkOrderStatus.Open;
    public bool CanSetWaitingApproval => SelectedStatus == WorkOrderStatus.InProgress;
    public bool CanSetCompleted => SelectedStatus == WorkOrderStatus.InProgress || SelectedStatus == WorkOrderStatus.WaitingApproval;
    public bool CanSetPaid => SelectedStatus == WorkOrderStatus.Completed;
    public bool CanOpenInspection =>
        SelectedStatus == WorkOrderStatus.Open ||
        SelectedStatus == WorkOrderStatus.InProgress ||
        SelectedStatus == WorkOrderStatus.WaitingApproval;
    private void RefreshWorkflowCommands()
    {
        OpenInspectionCommand.RaiseCanExecuteChanged();
        SetOpenCommand.RaiseCanExecuteChanged();
        SetInProgressCommand.RaiseCanExecuteChanged();
        SetWaitingApprovalCommand.RaiseCanExecuteChanged();
        SetCompletedCommand.RaiseCanExecuteChanged();
        SetPaidCommand.RaiseCanExecuteChanged();
    }
    public ObservableCollection<Technician> Technicians { get; } = new();

    private Technician? _selectedTechnician;
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
    private void LoadTechnicians()
    {
        Technicians.Clear();

        foreach (var tech in _workOrderService.GetTechnicians())
        {
            Technicians.Add(tech);
        }
    }
}