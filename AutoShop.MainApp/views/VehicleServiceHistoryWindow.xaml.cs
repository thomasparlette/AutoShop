using AutoShop.MainApp.ViewModels;
using System.Windows;

namespace AutoShop.MainApp.Views;

public partial class VehicleServiceHistoryWindow : Window
{
    public VehicleServiceHistoryWindow(int vehicleId)
    {
        InitializeComponent();
        DataContext = new VehicleServiceHistoryViewModel(vehicleId);
    }
}