using AutoShop.MainApp.ViewModels;
using System.Windows.Controls;

namespace AutoShop.MainApp.Views;

public partial class CustomerView : UserControl
{
    public CustomerView()
    {
        InitializeComponent();
        DataContext = new CustomerViewModel();
    }
}