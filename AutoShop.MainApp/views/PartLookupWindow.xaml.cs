using AutoShop.Core.Entities;
using AutoShop.MainApp.ViewModels;
using System.Windows;

namespace AutoShop.MainApp.Views;

public partial class PartLookupWindow : Window
{
    public Part? SelectedPart => (DataContext as PartLookupViewModel)?.SelectedPart;

    public PartLookupWindow()
    {
        InitializeComponent();
        DataContext = new PartLookupViewModel();
    }

    private void Select_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedPart == null)
        {
            MessageBox.Show("Please select a part first.");
            return;
        }

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}