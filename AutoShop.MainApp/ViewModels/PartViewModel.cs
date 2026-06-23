using AutoShop.Core.Entities;
using AutoShop.Data;
using AutoShop.MainApp.Helpers;
using AutoShop.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
namespace AutoShop.MainApp.ViewModels;

public class PartViewModel : INotifyPropertyChanged, IRefreshable
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

            if (value != null)
            {
                CurrentPart = ClonePart(value);
            }
            CostText = CurrentPart.Cost.ToString("0.00");
            SellPriceText = CurrentPart.SellPrice.ToString("0.00");
        }
    }

    private Part _currentPart = new();
    public Part CurrentPart
    {
        get => _currentPart;
        set
        {
            _currentPart = value;
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
    private string _costText = string.Empty;
    public string CostText
    {
        get => _costText;
        set
        {
            _costText = value;
            OnPropertyChanged();

            if (decimal.TryParse(_costText, out var cost))
            {
                var markup = GetPartMarkupPercent();
                SellPriceText = CalculateSellPrice(cost, markup).ToString("0.00");
            }
            else
            {
                SellPriceText = string.Empty;
            }
        }
    }

    private string _sellPriceText = string.Empty;
    public string SellPriceText
    {
        get => _sellPriceText;
        set
        {
            _sellPriceText = value;
            OnPropertyChanged();
        }
    }

    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand RefreshCommand { get; }

    public PartViewModel()
    {
        NewCommand = new RelayCommand(NewPart);
        SaveCommand = new RelayCommand(SavePart);
        DeleteCommand = new RelayCommand(DeletePart);
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

        LoadLowStockParts();
    }

    private void NewPart()
    {
        SelectedPart = null;
        CurrentPart = new Part
        {
            Active = true
        };
        CostText = string.Empty;
        SellPriceText = string.Empty;
    }

    private void SavePart()
    {
        if (string.IsNullOrWhiteSpace(CurrentPart.PartNumber))
            return;

        if (decimal.TryParse(CostText, out var cost))
        {
            CurrentPart.Cost = cost;
            CurrentPart.SellPrice = CalculateSellPrice(cost, GetPartMarkupPercent());
        }

        if (!decimal.TryParse(SellPriceText, NumberStyles.Number, CultureInfo.CurrentCulture, out var sellPrice) &&
            !decimal.TryParse(SellPriceText, NumberStyles.Number, CultureInfo.InvariantCulture, out sellPrice))
        {
            sellPrice = 0m;
        }
        CurrentPart.Cost = cost;
        CurrentPart.SellPrice = sellPrice;
        CurrentPart.UpdatedAt = DateTime.Now;

        _partService.SavePart(CurrentPart);
        LoadParts();
        NewPart();
    }

    private void DeletePart()
    {
        if (CurrentPart.Id == 0)
            return;

        _partService.DeletePart(CurrentPart.Id);
        LoadParts();
        NewPart();
    }

    private static Part ClonePart(Part part)
    {
        return new Part
        {
            Id = part.Id,
            PartNumber = part.PartNumber,
            Description = part.Description,
            Cost = part.Cost,
            SellPrice = part.SellPrice,
            QuantityOnHand = part.QuantityOnHand,
            ReorderLevel = part.ReorderLevel,
            Supplier = part.Supplier,
            Active = part.Active,
            CreatedAt = part.CreatedAt,
            UpdatedAt = part.UpdatedAt
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Refresh()
    {
        LoadParts();
    }
    private decimal GetPartMarkupPercent()
    {
        using var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>());
        return db.ShopSettings.FirstOrDefault()?.PartMarkupPercent ?? 0m;
    }

    private static decimal CalculateSellPrice(decimal cost, decimal markupPercent)
    {
        return cost + (cost * markupPercent / 100m);
    }
    public ObservableCollection<Part> LowStockParts { get; } = new();

    private void LoadLowStockParts()
    {
        LowStockParts.Clear();

        foreach (var part in _partService.GetLowStockParts())
        {
            LowStockParts.Add(part);
        }
    }
}