using AutoShop.MainApp.ViewModels;
using System.Windows.Controls;

namespace AutoShop.MainApp.Views;

public partial class TechnicianDashboardView : UserControl
{
    public TechnicianDashboardView()
    {
        InitializeComponent();
        DataContext = new TechnicianDashboardViewModel();
    }
}