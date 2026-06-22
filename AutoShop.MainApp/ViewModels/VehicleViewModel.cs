using AutoShop.Core.Entities;
using AutoShop.MainApp.Helpers;
using AutoShop.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AutoShop.MainApp.ViewModels;

public class VehicleViewModel : INotifyPropertyChanged, IRefreshable
{
    private readonly VehicleService _vehicleService = new();

    public ObservableCollection<Vehicle> Vehicles { get; } = new();
    public ObservableCollection<Customer> Customers { get; } = new();

    private int _selectedCustomerId;
    public int SelectedCustomerId
    {
        get => _selectedCustomerId;
        set
        {
            _selectedCustomerId = value;
            OnPropertyChanged();
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
                CurrentVehicle = CloneVehicle(value);
                SelectedCustomerId = value.CustomerId;
            }
        }
    }

    private Vehicle _currentVehicle = new();
    public Vehicle CurrentVehicle
    {
        get => _currentVehicle;
        set
        {
            _currentVehicle = value;
            OnPropertyChanged();
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
    public ICommand OpenServiceHistoryCommand { get; }
    public VehicleViewModel()
    {
        NewCommand = new RelayCommand(NewVehicle);
        SaveCommand = new RelayCommand(SaveVehicle);
        DeleteCommand = new RelayCommand(DeleteVehicle);
        RefreshCommand = new RelayCommand(LoadVehicles);
        OpenServiceHistoryCommand = new RelayCommand(OpenServiceHistory);
        LoadCustomers();
        LoadVehicles();
    }

    private void LoadCustomers()
    {
        Customers.Clear();

        foreach (var customer in _vehicleService.GetCustomers())
        {
            Customers.Add(customer);
        }
    }

    private void LoadVehicles()
    {
        Vehicles.Clear();

        foreach (var vehicle in _vehicleService.GetVehicles(SearchText))
        {
            Vehicles.Add(vehicle);
        }
    }

    private void NewVehicle()
    {
        SelectedVehicle = null;
        SelectedCustomerId = 0;
        CurrentVehicle = new Vehicle();
    }

    private void SaveVehicle()
    {
        CurrentVehicle.CustomerId = SelectedCustomerId;
        _vehicleService.SaveVehicle(CurrentVehicle);
        LoadVehicles();
        NewVehicle();
    }

    private void DeleteVehicle()
    {
        if (CurrentVehicle.Id == 0)
            return;

        _vehicleService.DeleteVehicle(CurrentVehicle.Id);
        LoadVehicles();
        NewVehicle();
    }
    private void OpenServiceHistory()
    {
        if (SelectedVehicle == null)
            return;

        var window = new AutoShop.MainApp.Views.VehicleServiceHistoryWindow(SelectedVehicle.Id);
        window.ShowDialog();
    }
    private static Vehicle CloneVehicle(Vehicle vehicle)
    {
        return new Vehicle
        {
            Id = vehicle.Id,
            CustomerId = vehicle.CustomerId,
            Year = vehicle.Year,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Trim = vehicle.Trim,
            Vin = vehicle.Vin,
            LicensePlate = vehicle.LicensePlate,
            Color = vehicle.Color,
            Mileage = vehicle.Mileage,
            Notes = vehicle.Notes
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Refresh()
    {
        LoadCustomers();
        LoadVehicles();
    }
}