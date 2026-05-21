namespace ChefsTest6.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IUserService _users;
    private readonly IAppSettingsService _settings;

    [ObservableProperty]
    private string fullName;

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string phoneNumber;

    [ObservableProperty]
    private bool notificationsEnabled;

    [ObservableProperty]
    private bool nightMode;

    [ObservableProperty]
    private string? statusMessage;

    public SettingsViewModel(INavigator navigator, IUserService users, IAppSettingsService settings)
    {
        _navigator = navigator;
        _users = users;
        _settings = settings;
        var u = users.Current;
        fullName = u.FullName ?? string.Empty;
        email = u.Email ?? string.Empty;
        phoneNumber = u.PhoneNumber ?? string.Empty;
        notificationsEnabled = settings.NotificationsEnabled;
        nightMode = settings.NightMode;
    }

    partial void OnNightModeChanged(bool value)
    {
        _settings.NightMode = value;
    }

    [RelayCommand]
    private void Save()
    {
        _users.UpdateCurrent(FullName, Email, PhoneNumber);
        _settings.NotificationsEnabled = NotificationsEnabled;
        StatusMessage = "Saved.";
    }

    [RelayCommand]
    private async Task LogOut()
    {
        await _navigator.NavigateViewModelAsync<LoginViewModel>(this, qualifier: Qualifiers.ClearBackStack);
    }
}
