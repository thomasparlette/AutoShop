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
    public ObservableCollection<Part> LowStockParts { get; } = new();

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

                CostText = CurrentPart.Cost.ToString("0.00", CultureInfo.CurrentCulture);
                SellPriceText = CurrentPart.SellPrice.ToString("0.00", CultureInfo.CurrentCulture);
                QuantityOnHandText = CurrentPart.QuantityOnHand.ToString(CultureInfo.CurrentCulture);
                ReorderLevelText = CurrentPart.ReorderLevel.ToString(CultureInfo.CurrentCulture);
            }
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

            if (decimal.TryParse(_costText, NumberStyles.Number, CultureInfo.CurrentCulture, out var cost) ||
                decimal.TryParse(_costText, NumberStyles.Number, CultureInfo.InvariantCulture, out cost))
            {
                SellPriceText = CalculateSellPrice(cost, GetPartMarkupPercent()).ToString("0.00", CultureInfo.CurrentCulture);
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

    private string _quantityOnHandText = "0";
    public string QuantityOnHandText
    {
        get => _quantityOnHandText;
        set
        {
            _quantityOnHandText = value;
            OnPropertyChanged();
        }
    }

    private string _reorderLevelText = "0";
    public string ReorderLevelText
    {
        get => _reorderLevelText;
        set
        {
            _reorderLevelText = value;
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
        NewPart();
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

    private void LoadLowStockParts()
    {
        LowStockParts.Clear();

        foreach (var part in _partService.GetLowStockParts())
        {
            LowStockParts.Add(part);
        }
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
        QuantityOnHandText = "0";
        ReorderLevelText = "0";
    }

    private void SavePart()
    {
        if (string.IsNullOrWhiteSpace(CurrentPart.PartNumber))
            return;

        CurrentPart.Cost = ParseDecimal(CostText);
        CurrentPart.SellPrice = CalculateSellPrice(CurrentPart.Cost, GetPartMarkupPercent());
        CurrentPart.QuantityOnHand = ParseInt(QuantityOnHandText);
        CurrentPart.ReorderLevel = ParseInt(ReorderLevelText);
        CurrentPart.UpdatedAt = DateTime.Now;

        if (CurrentPart.Id == 0)
        {
            CurrentPart.CreatedAt = DateTime.Now;
        }

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

    private decimal GetPartMarkupPercent()
    {
        using var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>());
        return db.ShopSettings.FirstOrDefault()?.PartMarkupPercent ?? 0m;
    }

    private static decimal CalculateSellPrice(decimal cost, decimal markupPercent)
    {
        return cost + (cost * markupPercent / 100m);
    }

    private static decimal ParseDecimal(string text)
    {
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out var value))
            return value;

        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            return value;

        return 0m;
    }

    private static int ParseInt(string text)
    {
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var value))
            return value;

        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            return value;

        return 0;
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
}