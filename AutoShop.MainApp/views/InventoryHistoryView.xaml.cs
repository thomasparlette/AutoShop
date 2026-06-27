using AutoShop.MainApp.ViewModels;
using System.Windows.Controls;

namespace AutoShop.MainApp.Views;

public partial class InventoryHistoryView : UserControl
{
    public InventoryHistoryView()
    {
        InitializeComponent();
        DataContext = new InventoryHistoryViewModel();
    }
}