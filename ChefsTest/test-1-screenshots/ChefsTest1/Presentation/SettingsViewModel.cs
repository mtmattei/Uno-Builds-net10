namespace ChefsTest1.Presentation;

public partial class SettingsViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IUserService _users;

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string mobile = string.Empty;
    [ObservableProperty] private bool notificationsEnabled = true;
    [ObservableProperty] private bool nightMode;

    public SettingsViewModel(INavigator navigator, IUserService users)
    {
        _navigator = navigator;
        _users = users;
        BackCommand = new AsyncRelayCommand(() => _navigator.NavigateBackAsync(this));
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        LogOutCommand = new AsyncRelayCommand(() => _navigator.NavigateRouteAsync(this, "-/Login"));
        _ = LoadAsync();
    }

    public ICommand BackCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand LogOutCommand { get; }

    private async Task LoadAsync()
    {
        var user = await _users.GetCurrentAsync();
        if (user != null)
        {
            Name = user.FullName ?? "Jenna Smith";
            Email = string.IsNullOrEmpty(user.Email) ? "Jenna.Smith@platform.uno" : user.Email;
            Mobile = user.PhoneNumber ?? string.Empty;
        }
    }

    partial void OnNightModeChanged(bool value)
    {
        if (Application.Current is App)
        {
            try
            {
                if (Application.Current.Resources.MergedDictionaries.Count > 0)
                {
                    var elt = Microsoft.UI.Xaml.Window.Current?.Content as FrameworkElement;
                    if (elt != null)
                    {
                        elt.RequestedTheme = value ? ElementTheme.Dark : ElementTheme.Light;
                    }
                }
            }
            catch { }
        }
    }

    private async Task SaveAsync()
    {
        await Task.CompletedTask;
    }
}
