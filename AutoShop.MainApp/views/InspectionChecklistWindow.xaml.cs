using AutoShop.MainApp.ViewModels;
using System.Windows;

namespace AutoShop.MainApp.Views;

public partial class InspectionChecklistWindow : Window
{
    public InspectionChecklistWindow(int workOrderId)
    {
        InitializeComponent();
        DataContext = new InspectionChecklistViewModel(workOrderId);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}