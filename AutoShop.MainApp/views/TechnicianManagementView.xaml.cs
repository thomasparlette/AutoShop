using AutoShop.MainApp.ViewModels;
using System.Windows.Controls;

namespace AutoShop.MainApp.Views;

public partial class TechnicianManagementView : UserControl
{
    public TechnicianManagementView()
    {
        InitializeComponent();
        DataContext = new TechnicianManagementViewModel();
    }
}