using ChefsTest3.Models;
using ChefsTest3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Uno.Extensions.Navigation;

namespace ChefsTest3.Presentation;

public partial class SettingsViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsService _service;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private bool _notificationsEnabled = true;

    [ObservableProperty]
    private bool _nightMode;

    public SettingsViewModel(INavigator navigator, IChefsService service)
    {
        _navigator = navigator;
        _service = service;
        _ = LoadAsync();
        var current = Application.Current?.RequestedTheme;
        NightMode = current == ApplicationTheme.Dark;
    }

    private async System.Threading.Tasks.Task LoadAsync()
    {
        var user = await _service.GetCurrentUserAsync();
        Name = user.FullName ?? string.Empty;
        Email = user.Email ?? string.Empty;
        PhoneNumber = user.PhoneNumber ?? string.Empty;
    }

    partial void OnNightModeChanged(bool value)
    {
        App.SetTheme(value);
    }

    [RelayCommand]
    private async void SaveChanges()
    {
        var user = await _service.GetCurrentUserAsync();
        user.FullName = Name;
        user.Email = Email;
        user.PhoneNumber = PhoneNumber;
        await _service.UpdateCurrentUserAsync(user);
        await _navigator.NavigateBackAsync(this);
    }

    [RelayCommand]
    private async void Logout()
    {
        await _navigator.NavigateRouteAsync(this, "Login", qualifier: Qualifiers.ClearBackStack);
    }

    [RelayCommand]
    private async void GoBack()
    {
        await _navigator.NavigateBackAsync(this);
    }
}
