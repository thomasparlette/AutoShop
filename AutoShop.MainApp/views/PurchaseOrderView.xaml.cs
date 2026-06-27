using AutoShop.MainApp.ViewModels;
using System.Windows.Controls;

namespace AutoShop.MainApp.Views;

public partial class PurchaseOrderView : UserControl
{
    public PurchaseOrderView()
    {
        InitializeComponent();
        DataContext = new PurchaseOrderViewModel();
    }

    private void LineItemsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (DataContext is PurchaseOrderViewModel vm)
        {
            vm.RefreshTotals();
        }
    }
}