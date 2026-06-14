using AutoShop.MainApp.Services;
using AutoShop.MainApp.Views;
using AutoShop.Services;
using System.Windows;

namespace AutoShop.MainApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        base.OnStartup(e);

        var authService = new AuthService();
        authService.EnsureDefaults();

        var loginWindow = new LoginWindow();
        var loginResult = loginWindow.ShowDialog();

        if (loginResult != true || AppSession.CurrentUser == null)
        {
            Shutdown();
            return;
        }

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        mainWindow.Show();
    }
}