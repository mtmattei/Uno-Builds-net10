namespace ChefsTest6.Presentation;

public partial class LoginViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefService _chef;

    [ObservableProperty]
    private string? username = "james.bondi@gmail.com";

    [ObservableProperty]
    private string? password = "123";

    [ObservableProperty]
    private bool rememberMe = true;

    [ObservableProperty]
    private string? errorMessage;

    public LoginViewModel(INavigator navigator, IChefService chef)
    {
        _navigator = navigator;
        _chef = chef;
    }

    [RelayCommand]
    private async Task SignInAsync()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username and password are required.";
            return;
        }

        var user = await _chef.AuthenticateAsync(new LoginRequest(Username, Password));
        if (user is null)
        {
            ErrorMessage = "Invalid credentials.";
            return;
        }
        await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);
    }

    [RelayCommand]
    private async Task SocialAsync(string provider)
    {
        // Visual-only affordance per PRD §F2.3. Demo bypass: signs in as the seeded user.
        await SignInAsync();
    }

    [RelayCommand]
    private void Forgot()
    {
        ErrorMessage = string.IsNullOrWhiteSpace(Username)
            ? "Enter your email above and we'll send a reset link."
            : $"Reset link sent to {Username}. (Demo build — no email actually delivered.)";
    }

    [RelayCommand]
    private async Task GoToRegisterAsync()
    {
        await _navigator.NavigateViewModelAsync<RegisterViewModel>(this);
    }
}
