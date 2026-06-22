using AutoShop.MainApp.ViewModels;
using System.Windows.Controls;

namespace AutoShop.MainApp.Views;

public partial class PartView : UserControl
{
    public PartView()
    {
        InitializeComponent();
        DataContext = new PartViewModel();
    }
}