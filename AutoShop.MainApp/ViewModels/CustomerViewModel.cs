using AutoShop.Core.Entities;
using AutoShop.MainApp.Helpers;
using AutoShop.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AutoShop.MainApp.ViewModels;

public class CustomerViewModel : INotifyPropertyChanged, IRefreshable
{
    private readonly CustomerService _customerService = new();

    public ObservableCollection<Customer> Customers { get; } = new();

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
                CurrentCustomer = CloneCustomer(value);
            }
        }
    }

    private Customer _currentCustomer = new();
    public Customer CurrentCustomer
    {
        get => _currentCustomer;
        set
        {
            _currentCustomer = value;
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

    public CustomerViewModel()
    {
        NewCommand = new RelayCommand(NewCustomer);
        SaveCommand = new RelayCommand(SaveCustomer);
        DeleteCommand = new RelayCommand(DeleteCustomer);
        RefreshCommand = new RelayCommand(LoadCustomers);

        LoadCustomers();
    }

    private void LoadCustomers()
    {
        Customers.Clear();

        foreach (var customer in _customerService.GetCustomers(SearchText))
        {
            Customers.Add(customer);
        }
    }

    private void NewCustomer()
    {
        SelectedCustomer = null;
        CurrentCustomer = new Customer();
    }

    private void SaveCustomer()
    {
        _customerService.SaveCustomer(CurrentCustomer);
        LoadCustomers();
        NewCustomer();
    }

    private void DeleteCustomer()
    {
        if (CurrentCustomer.Id == 0)
            return;

        _customerService.DeleteCustomer(CurrentCustomer.Id);
        LoadCustomers();
        NewCustomer();
    }

    private static Customer CloneCustomer(Customer customer)
    {
        return new Customer
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Phone = customer.Phone,
            Email = customer.Email,
            AddressLine1 = customer.AddressLine1,
            AddressLine2 = customer.AddressLine2,
            City = customer.City,
            State = customer.State,
            PostalCode = customer.PostalCode,
            Notes = customer.Notes
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
    }
}