using Uno.Toolkit.UI;

namespace ChefsTest5.Presentation;

public partial class SettingsViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;
    private readonly IThemeService? _themeService;

    [ObservableProperty] private string userName = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string phoneNumber = string.Empty;
    [ObservableProperty] private bool notificationsEnabled = true;
    [ObservableProperty] private bool nightMode;

    public IAsyncRelayCommand BackCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand LogoutCommand { get; }

    public SettingsViewModel(INavigator navigator, IChefsDataService data, IServiceProvider sp)
    {
        _navigator = navigator;
        _data = data;
        _themeService = sp.GetService(typeof(IThemeService)) as IThemeService;
        nightMode = _data.IsNightMode;

        _ = LoadAsync();

        BackCommand = new AsyncRelayCommand(async () => await _navigator.NavigateBackAsync(this));
        SaveCommand = new AsyncRelayCommand(async () => await _navigator.NavigateBackAsync(this));
        LogoutCommand = new AsyncRelayCommand(async () =>
            await _navigator.NavigateViewModelAsync<LoginViewModel>(this, qualifier: Qualifiers.ClearBackStack));
    }

    private async Task LoadAsync()
    {
        var user = await _data.GetCurrentUserAsync();
        if (user is not null)
        {
            UserName = user.FullName ?? string.Empty;
            Email = user.Email ?? string.Empty;
            PhoneNumber = user.PhoneNumber ?? string.Empty;
        }
    }

    partial void OnNightModeChanged(bool value)
    {
        _ = _data.SetNightModeAsync(value);
        if (_themeService is not null)
        {
            _ = _themeService.SetThemeAsync(value ? AppTheme.Dark : AppTheme.Light);
        }
    }
}
