using AutoShop.Core.Entities;
using AutoShop.MainApp.Helpers;
using AutoShop.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AutoShop.MainApp.ViewModels;

public class PartLookupViewModel : INotifyPropertyChanged, IRefreshable
{
    private readonly PartService _partService = new();

    public ObservableCollection<Part> Parts { get; } = new();

    private Part? _selectedPart;
    public Part? SelectedPart
    {
        get => _selectedPart;
        set
        {
            _selectedPart = value;
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

    public ICommand RefreshCommand { get; }

    public PartLookupViewModel()
    {
        RefreshCommand = new RelayCommand(LoadParts);
        LoadParts();
    }

    private void LoadParts()
    {
        Parts.Clear();

        foreach (var part in _partService.GetParts(ShowInactive, SearchText))
        {
            Parts.Add(part);
        }
    }

    public void Refresh() => LoadParts();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}