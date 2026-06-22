using AutoShop.Core.Entities;
using AutoShop.MainApp.Helpers;
using AutoShop.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoShop.MainApp.ViewModels;

public class VehicleServiceHistoryViewModel : INotifyPropertyChanged
{
    private readonly VehicleService _vehicleService = new();
    private readonly WorkOrderService _workOrderService = new();

    public ObservableCollection<WorkOrder> ServiceHistory { get; } = new();

    private Vehicle? _vehicle;
    public Vehicle? Vehicle
    {
        get => _vehicle;
        set
        {
            _vehicle = value;
            OnPropertyChanged();
        }
    }

    public VehicleServiceHistoryViewModel(int vehicleId)
    {
        Load(vehicleId);
    }

    private void Load(int vehicleId)
    {
        Vehicle = _vehicleService.GetVehicleById(vehicleId);
        ServiceHistory.Clear();

        foreach (var wo in _workOrderService.GetServiceHistoryForVehicle(vehicleId))
        {
            ServiceHistory.Add(wo);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}