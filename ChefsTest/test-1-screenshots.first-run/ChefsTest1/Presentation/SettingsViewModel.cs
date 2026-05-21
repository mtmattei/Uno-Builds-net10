using System.Threading.Tasks;
using ChefsTest1.Models;
using ChefsTest1.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class SettingsViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private string? name;
    [ObservableProperty] private string? email;
    [ObservableProperty] private string? phone;
    [ObservableProperty] private bool notificationsEnabled = true;
    [ObservableProperty] private bool nightMode;

    public SettingsViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        _ = Load();
    }

    private async Task Load()
    {
        var u = await _data.GetCurrentUserAsync();
        Name = u.FullName;
        Email = u.Email;
        Phone = u.PhoneNumber;
    }

    partial void OnNightModeChanged(bool value)
    {
        if (Application.Current is App app && app.MainWindow?.Content is FrameworkElement root)
            root.RequestedTheme = value ? ElementTheme.Dark : ElementTheme.Light;
    }

    [RelayCommand] private Task Back() => _navigator.NavigateBackAsync(this);
    [RelayCommand] private Task Save() => _navigator.NavigateBackAsync(this);
    [RelayCommand] private Task Logout() => _navigator.NavigateViewModelAsync<LoginViewModel>(this, qualifier: Qualifiers.ClearBackStack);
}
