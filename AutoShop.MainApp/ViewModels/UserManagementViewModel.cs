using AutoShop.Core.Entities;
using AutoShop.Core.Enums;
using AutoShop.MainApp.Helpers;
using AutoShop.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace AutoShop.MainApp.ViewModels;

public class UserManagementViewModel : INotifyPropertyChanged, IRefreshable
{
    private readonly UserService _userService = new();

    public ObservableCollection<AppUser> Users { get; } = new();

    private AppUser? _selectedUser;
    public AppUser? SelectedUser
    {
        get => _selectedUser;
        set
        {
            _selectedUser = value;
            OnPropertyChanged();

            if (value != null)
            {
                CurrentUser = CloneUser(value);
                Password = string.Empty;
                ConfirmPassword = string.Empty;
                SetRoleFlags(value.Role);
            }
        }
    }

    private AppUser _currentUser = new();
    public AppUser CurrentUser
    {
        get => _currentUser;
        set
        {
            _currentUser = value;
            OnPropertyChanged();
        }
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
        }
    }

    private string _confirmPassword = string.Empty;
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            _confirmPassword = value;
            OnPropertyChanged();
        }
    }

    private bool _isTechnicianRole;
    public bool IsTechnicianRole
    {
        get => _isTechnicianRole;
        set
        {
            _isTechnicianRole = value;
            OnPropertyChanged();
            SyncRolesToCurrentUser();
        }
    }

    private bool _isFinanceRole;
    public bool IsFinanceRole
    {
        get => _isFinanceRole;
        set
        {
            _isFinanceRole = value;
            OnPropertyChanged();
            SyncRolesToCurrentUser();
        }
    }

    private bool _isAdminRole;
    public bool IsAdminRole
    {
        get => _isAdminRole;
        set
        {
            _isAdminRole = value;
            OnPropertyChanged();
            SyncRolesToCurrentUser();
        }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
        }
    }

    private bool _showInactive;
    public bool ShowInactive
    {
        get => _showInactive;
        set
        {
            _showInactive = value;
            OnPropertyChanged();
        }
    }

    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeactivateCommand { get; }
    public ICommand RefreshCommand { get; }

    public UserManagementViewModel()
    {
        NewCommand = new RelayCommand(NewUser);
        SaveCommand = new RelayCommand(SaveUser);
        DeactivateCommand = new RelayCommand(DeactivateUser);
        RefreshCommand = new RelayCommand(LoadUsers);

        LoadUsers();
    }

    private void LoadUsers()
    {
        Users.Clear();

        foreach (var user in _userService.GetUsers(ShowInactive, SearchText))
        {
            Users.Add(user);
        }
    }

    private void NewUser()
    {
        SelectedUser = null;
        CurrentUser = new AppUser
        {
            IsActive = true
        };
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        SetRoleFlags(UserRole.None);
    }

    private void SaveUser()
    {
        if (string.IsNullOrWhiteSpace(CurrentUser.UserName))
        {
            MessageBox.Show("Username is required.");
            return;
        }

        if (CurrentUser.Id == 0 && string.IsNullOrWhiteSpace(Password))
        {
            MessageBox.Show("A password is required for a new user.");
            return;
        }

        if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
        {
            MessageBox.Show("Passwords do not match.");
            return;
        }

        _userService.SaveUser(CurrentUser, string.IsNullOrWhiteSpace(Password) ? null : Password);
        new TechnicianService().SyncTechniciansFromUsers();
        LoadUsers();
        NewUser();
    }

    private void DeactivateUser()
    {
        if (CurrentUser.Id == 0)
            return;

        _userService.DeactivateUser(CurrentUser.Id);
        LoadUsers();
        NewUser();
    }

    private static AppUser CloneUser(AppUser user)
    {
        return new AppUser
        {
            Id = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            PasswordHash = user.PasswordHash,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    private void SetRoleFlags(UserRole role)
    {
        _isTechnicianRole = role.HasFlag(UserRole.Technician);
        _isFinanceRole = role.HasFlag(UserRole.Finance);
        _isAdminRole = role.HasFlag(UserRole.Admin);

        OnPropertyChanged(nameof(IsTechnicianRole));
        OnPropertyChanged(nameof(IsFinanceRole));
        OnPropertyChanged(nameof(IsAdminRole));
    }

    private void SyncRolesToCurrentUser()
    {
        if (CurrentUser == null)
            return;

        var roles = UserRole.None;

        if (IsTechnicianRole)
            roles |= UserRole.Technician;

        if (IsFinanceRole)
            roles |= UserRole.Finance;

        if (IsAdminRole)
            roles |= UserRole.Admin;

        CurrentUser.Role = roles;
        OnPropertyChanged(nameof(CurrentUser));
    }

    public void Refresh()
    {
        LoadUsers();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}