using AutoShop.MainApp.Helpers;
using AutoShop.MainApp.Services;
using System.Windows;
using System.Windows.Controls;

namespace AutoShop.MainApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        SettingsTab.Visibility = AppSession.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        TechniciansTab.Visibility = AppSession.IsAdmin ? Visibility.Visible : Visibility.Collapsed;

        MainTabs.SelectionChanged += MainTabs_SelectionChanged;
    }

    private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.Source, MainTabs))
            return;

        if (MainTabs.SelectedContent is not FrameworkElement element)
            return;

        if (element.DataContext is IRefreshable refreshable)
        {
            refreshable.Refresh();
        }
    }
}