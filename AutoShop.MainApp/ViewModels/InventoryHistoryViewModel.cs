using AutoShop.Core.Entities;
using AutoShop.MainApp.Helpers;
using AutoShop.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AutoShop.MainApp.ViewModels;

public class InventoryHistoryViewModel : INotifyPropertyChanged, IRefreshable
{
    private readonly PartService _partService = new();

    public ObservableCollection<InventoryTransaction> Transactions { get; } = new();

    private Part? _selectedPart;
    public Part? SelectedPart
    {
        get => _selectedPart;
        set
        {
            _selectedPart = value;
            OnPropertyChanged();

            if (value != null)
            {
                LoadTransactions(value.Id);
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

    public ObservableCollection<Part> Parts { get; } = new();

    public ICommand RefreshCommand { get; }

    public InventoryHistoryViewModel()
    {
        RefreshCommand = new RelayCommand(Refresh);
        LoadParts();
    }

    private void LoadParts()
    {
        Parts.Clear();
        foreach (var part in _partService.GetParts(false, SearchText))
        {
            Parts.Add(part);
        }
    }

    private void LoadTransactions(int partId)
    {
        Transactions.Clear();

        foreach (var tx in _partService.GetTransactionsForPart(partId))
        {
            Transactions.Add(tx);
        }
    }

    public void Refresh()
    {
        LoadParts();

        if (SelectedPart != null)
            LoadTransactions(SelectedPart.Id);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}