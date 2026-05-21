namespace ChefsTest6.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IUserService _users;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool rememberMe = true;

    [ObservableProperty]
    private string? errorMessage;

    public LoginViewModel(INavigator navigator, IUserService users)
    {
        _navigator = navigator;
        _users = users;
    }

    [RelayCommand]
    private async Task SignIn()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username and password are required.";
            return;
        }

        if (_users.Authenticate(Username, Password, out _))
        {
            await _navigator.NavigateViewModelAsync<HomeViewModel>(this, qualifier: Qualifiers.ClearBackStack);
        }
        else
        {
            ErrorMessage = "Sign in failed.";
        }
    }

    [RelayCommand]
    private async Task GoToRegister()
    {
        await _navigator.NavigateViewModelAsync<RegisterViewModel>(this);
    }

    [RelayCommand]
    private void ForgotPassword()
    {
        // No-op per PRD: visual only in v1.
        ErrorMessage = "Reset link sent (demo).";
    }

    [RelayCommand]
    private void SignInWithSocial(string provider)
    {
        ErrorMessage = $"{provider} sign-in is visual only in v1.";
    }
}
