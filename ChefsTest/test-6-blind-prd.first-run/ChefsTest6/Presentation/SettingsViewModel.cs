namespace ChefsTest6.Presentation;

public partial class SettingsViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefService _chef;
    private readonly IAppThemeService _theme;

    [ObservableProperty]
    private string? fullName;

    [ObservableProperty]
    private string? email;

    [ObservableProperty]
    private string? phoneNumber;

    [ObservableProperty]
    private bool notificationsEnabled = true;

    [ObservableProperty]
    private bool nightMode;

    [ObservableProperty]
    private string? statusMessage;

    public SettingsViewModel(INavigator navigator, IChefService chef, IAppThemeService theme)
    {
        _navigator = navigator;
        _chef = chef;
        _theme = theme;
        if (chef.CurrentUser is { } user)
        {
            FullName = user.FullName;
            Email = user.Email;
            PhoneNumber = user.PhoneNumber;
        }
        NightMode = theme.IsDark;
    }

    partial void OnNightModeChanged(bool value) => _theme.Set(value);

    partial void OnPhoneNumberChanged(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        var filtered = new string(value.Where(char.IsDigit).ToArray());
        if (filtered != value)
        {
            PhoneNumber = filtered;
        }
    }

    [RelayCommand]
    private void Save()
    {
        StatusMessage = null;
        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains('@'))
        {
            StatusMessage = "Email must be valid.";
            return;
        }
        if (_chef.CurrentUser is { } user)
        {
            _chef.UpdateCurrentUser(user with
            {
                FullName = FullName,
                Email = Email,
                PhoneNumber = PhoneNumber,
            });
        }
        StatusMessage = "Saved.";
    }

    [RelayCommand]
    private async Task LogoutAsync()
        => await _navigator.NavigateViewModelAsync<LoginViewModel>(this, qualifier: Qualifiers.ClearBackStack);

    [RelayCommand]
    private async Task BackAsync() => await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);
}
