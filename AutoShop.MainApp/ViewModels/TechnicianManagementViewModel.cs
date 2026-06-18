using AutoShop.Core.Entities;
using AutoShop.MainApp.Helpers;
using AutoShop.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AutoShop.MainApp.ViewModels;

public class TechnicianManagementViewModel : INotifyPropertyChanged, IRefreshable
{
    private readonly TechnicianService _technicianService = new();

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
                CurrentTechnician = CloneTechnician(value);
            }
        }
    }

    private Technician _currentTechnician = new();
    public Technician CurrentTechnician
    {
        get => _currentTechnician;
        set
        {
            _currentTechnician = value;
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

    private bool _showInactive;
    public bool ShowInactive
    {
        get => _showInactive;
        set
        {
            _showInactive = value;
            OnPropertyChanged();
        }
    }

    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeactivateCommand { get; }
    public ICommand RefreshCommand { get; }

    public TechnicianManagementViewModel()
    {
        NewCommand = new RelayCommand(NewTechnician);
        SaveCommand = new RelayCommand(SaveTechnician);
        DeactivateCommand = new RelayCommand(DeactivateTechnician);
        RefreshCommand = new RelayCommand(LoadTechnicians);

        LoadTechnicians();
    }

    private void LoadTechnicians()
    {
        Technicians.Clear();

        foreach (var tech in _technicianService.GetTechnicians(ShowInactive, SearchText))
        {
            Technicians.Add(tech);
        }
    }

    private void NewTechnician()
    {
        SelectedTechnician = null;
        CurrentTechnician = new Technician();
    }

    private void SaveTechnician()
    {
        _technicianService.SaveTechnician(CurrentTechnician);
        LoadTechnicians();
        NewTechnician();
    }

    private void DeactivateTechnician()
    {
        if (CurrentTechnician.Id == 0)
            return;

        _technicianService.DeactivateTechnician(CurrentTechnician.Id);
        LoadTechnicians();
        NewTechnician();
    }

    private static Technician CloneTechnician(Technician tech)
    {
        return new Technician
        {
            Id = tech.Id,
            FirstName = tech.FirstName,
            LastName = tech.LastName,
            Phone = tech.Phone,
            Active = tech.Active,
            LaborRate = tech.LaborRate
        };
    }

    public void Refresh()
    {
        LoadTechnicians();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}