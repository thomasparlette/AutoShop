using AutoShop.MainApp.ViewModels;
using System.Windows.Controls;

namespace AutoShop.MainApp.Views;

public partial class WorkOrderView : UserControl
{
    public WorkOrderView()
    {
        InitializeComponent();
        DataContext = new WorkOrderViewModel();
    }
}