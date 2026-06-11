using AutoShop.MainApp.ViewModels;
using System.Windows.Controls;

namespace AutoShop.MainApp.Views;

public partial class VehicleView : UserControl
{
    public VehicleView()
    {
        InitializeComponent();
        DataContext = new VehicleViewModel();
    }
}