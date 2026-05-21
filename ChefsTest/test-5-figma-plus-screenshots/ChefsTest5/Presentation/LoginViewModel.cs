namespace ChefsTest5.Presentation;

public partial class LoginViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private bool rememberMe;

    public IAsyncRelayCommand LoginCommand { get; }
    public IAsyncRelayCommand RegisterCommand { get; }

    public LoginViewModel(INavigator navigator)
    {
        _navigator = navigator;
        LoginCommand = new AsyncRelayCommand(async () =>
            await _navigator.NavigateViewModelAsync<MainShellViewModel>(this, qualifier: Qualifiers.ClearBackStack));
        RegisterCommand = new AsyncRelayCommand(async () =>
            await _navigator.NavigateViewModelAsync<RegisterViewModel>(this));
    }
}
