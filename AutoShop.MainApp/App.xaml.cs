using AutoShop.Data;
using AutoShop.MainApp.Views;
using AutoShop.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Windows;

namespace AutoShop.MainApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            using (var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>()))
            {
                db.Database.Migrate();
            }

            new AuthService().EnsureDefaults();

            var login = new LoginWindow();
            var loginResult = login.ShowDialog();

            if (loginResult == true)
            {
                var mainWindow = new MainWindow();
                MainWindow = mainWindow;
                ShutdownMode = ShutdownMode.OnMainWindowClose;
                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(1);
        }
    }
}