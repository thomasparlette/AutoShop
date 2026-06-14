using AutoShop.MainApp.ViewModels;
using System.Windows.Controls;

namespace AutoShop.MainApp.Views;

public partial class ShopSettingsView : UserControl
{
    public ShopSettingsView()
    {
        InitializeComponent();
        DataContext = new ShopSettingsViewModel();
    }
}