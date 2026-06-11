using AutoShop.MainApp.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace AutoShop.MainApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MainTabs.SelectionChanged += MainTabs_SelectionChanged;
    }

    private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MainTabs.SelectedContent is not FrameworkElement element)
            return;

        if (element.DataContext is IRefreshable refreshable)
        {
            refreshable.Refresh();
        }
    }
}