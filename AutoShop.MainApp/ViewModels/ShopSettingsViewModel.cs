using AutoShop.Core.Entities;
using AutoShop.MainApp.Helpers;
using AutoShop.Services;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AutoShop.MainApp.ViewModels;

public class ShopSettingsViewModel : INotifyPropertyChanged, IRefreshable
{
    private readonly AuthService _authService = new();

    private ShopSettings _settings = new();
    public ShopSettings Settings
    {
        get => _settings;
        set
        {
            _settings = value;
            OnPropertyChanged();
            NotifyAllBoundProperties();
        }
    }

    public string ShopName
    {
        get => Settings.ShopName;
        set { Settings.ShopName = value; OnPropertyChanged(); }
    }

    public string? LogoPath
    {
        get => Settings.LogoPath;
        set
        {
            Settings.LogoPath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LogoPreview));
        }
    }

    public string? LogoPreview => Settings.LogoPath;

    public string? AddressLine1
    {
        get => Settings.AddressLine1;
        set { Settings.AddressLine1 = value; OnPropertyChanged(); }
    }

    public string? AddressLine2
    {
        get => Settings.AddressLine2;
        set { Settings.AddressLine2 = value; OnPropertyChanged(); }
    }

    public string? City
    {
        get => Settings.City;
        set { Settings.City = value; OnPropertyChanged(); }
    }

    public string? State
    {
        get => Settings.State;
        set { Settings.State = value; OnPropertyChanged(); }
    }

    public string? PostalCode
    {
        get => Settings.PostalCode;
        set { Settings.PostalCode = value; OnPropertyChanged(); }
    }

    public string? Phone
    {
        get => Settings.Phone;
        set { Settings.Phone = value; OnPropertyChanged(); }
    }

    public string? Email
    {
        get => Settings.Email;
        set { Settings.Email = value; OnPropertyChanged(); }
    }

    public string? Website
    {
        get => Settings.Website;
        set { Settings.Website = value; OnPropertyChanged(); }
    }

    public decimal TaxRate
    {
        get => Settings.TaxRate;
        set { Settings.TaxRate = value; OnPropertyChanged(); }
    }

    public decimal DefaultLaborRate
    {
        get => Settings.DefaultLaborRate;
        set { Settings.DefaultLaborRate = value; OnPropertyChanged(); }
    }

    public string? ReceiptFooterText
    {
        get => Settings.ReceiptFooterText;
        set { Settings.ReceiptFooterText = value; OnPropertyChanged(); }
    }

    public string? DefaultThankYouMessage
    {
        get => Settings.DefaultThankYouMessage;
        set { Settings.DefaultThankYouMessage = value; OnPropertyChanged(); }
    }

    public ICommand SaveCommand { get; }
    public ICommand ReloadCommand { get; }
    public ICommand BrowseLogoCommand { get; }

    public ShopSettingsViewModel()
    {
        SaveCommand = new RelayCommand(SaveSettings);
        ReloadCommand = new RelayCommand(LoadSettings);
        BrowseLogoCommand = new RelayCommand(BrowseLogo);
        LoadSettings();
    }

    private void LoadSettings()
    {
        Settings = _authService.GetShopSettings();
    }

    private void SaveSettings()
    {
        _authService.SaveShopSettings(Settings);
        LoadSettings();
    }

    private void BrowseLogo()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Shop Logo",
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
            return;

        var appFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AutoShop",
            "Assets");

        Directory.CreateDirectory(appFolder);

        var fileName = Path.GetFileName(dialog.FileName);
        var destinationPath = Path.Combine(appFolder, fileName);

        File.Copy(dialog.FileName, destinationPath, true);
        LogoPath = destinationPath;
    }

    public void Refresh()
    {
        LoadSettings();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void NotifyAllBoundProperties()
    {
        OnPropertyChanged(nameof(ShopName));
        OnPropertyChanged(nameof(LogoPath));
        OnPropertyChanged(nameof(LogoPreview));
        OnPropertyChanged(nameof(AddressLine1));
        OnPropertyChanged(nameof(AddressLine2));
        OnPropertyChanged(nameof(City));
        OnPropertyChanged(nameof(State));
        OnPropertyChanged(nameof(PostalCode));
        OnPropertyChanged(nameof(Phone));
        OnPropertyChanged(nameof(Email));
        OnPropertyChanged(nameof(Website));
        OnPropertyChanged(nameof(TaxRate));
        OnPropertyChanged(nameof(DefaultLaborRate));
        OnPropertyChanged(nameof(ReceiptFooterText));
        OnPropertyChanged(nameof(DefaultThankYouMessage));
    }
}