using AutoShop.Core.Entities;
using AutoShop.MainApp.Helpers;
using AutoShop.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AutoShop.MainApp.ViewModels;

public class TechnicianDashboardViewModel : INotifyPropertyChanged, IRefreshable
{
    private readonly TechnicianService _technicianService = new();

    public ObservableCollection<Technician> Technicians { get; } = new();
    public ObservableCollection<WorkOrder> ActiveWorkOrders { get; } = new();

    private Technician? _selectedTechnician;
    public Technician? SelectedTechnician
    {
        get => _selectedTechnician;
        set
        {
            _selectedTechnician = value;
            OnPropertyChanged();
            LoadActiveWorkOrders();
        }
    }

    public ICommand RefreshCommand { get; }

    public TechnicianDashboardViewModel()
    {
        RefreshCommand = new RelayCommand(Refresh);
        LoadTechnicians();
    }

    private void LoadTechnicians()
    {
        Technicians.Clear();

        foreach (var tech in _technicianService.GetTechnicians())
        {
            Technicians.Add(tech);
        }

        if (SelectedTechnician == null && Technicians.Count > 0)
        {
            SelectedTechnician = Technicians[0];
        }
        else
        {
            LoadActiveWorkOrders();
        }
    }

    private void LoadActiveWorkOrders()
    {
        ActiveWorkOrders.Clear();

        if (SelectedTechnician == null)
            return;

        foreach (var workOrder in _technicianService.GetActiveWorkOrdersForTechnician(SelectedTechnician.Id))
        {
            ActiveWorkOrders.Add(workOrder);
        }
    }

    public void Refresh()
    {
        LoadTechnicians();
        LoadActiveWorkOrders();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}