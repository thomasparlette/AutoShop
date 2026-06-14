using AutoShop.MainApp.Services;
using AutoShop.Services;
using System.Windows;

namespace AutoShop.MainApp.Views;

public partial class LoginWindow : Window
{
    private readonly AuthService _authService = new();

    public LoginWindow()
    {
        InitializeComponent();
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        var userName = UserNameBox.Text;
        var password = PasswordBox.Password;

        var user = _authService.Authenticate(userName, password);
        if (user == null)
        {
            ErrorText.Text = "Invalid username or password.";
            return;
        }

        AppSession.CurrentUser = user;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}