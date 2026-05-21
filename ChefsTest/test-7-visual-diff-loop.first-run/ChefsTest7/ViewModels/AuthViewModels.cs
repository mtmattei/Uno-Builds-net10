using System.Threading.Tasks;
using ChefsTest7.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest7.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private string username = "james.bondi@gmail.com";
    [ObservableProperty] private string password = "123";
    [ObservableProperty] private bool rememberMe = true;
    [ObservableProperty] private string? errorMessage;

    public LoginViewModel(INavigator navigator, IChefsDataService data)
    { _navigator = navigator; _data = data; }

    [RelayCommand]
    private async Task Login()
    {
        var users = await _data.GetUsersAsync();
        var match = users.FirstOrDefault(u => string.Equals(u.Email, Username, StringComparison.OrdinalIgnoreCase) && u.Password == Password);
        if (match is null) { ErrorMessage = "Invalid credentials"; return; }
        ErrorMessage = null;
        await _navigator.NavigateRouteAsync(this, "Home");
    }

    [RelayCommand]
    private async Task LoginApple() => await _navigator.NavigateRouteAsync(this, "Home");

    [RelayCommand]
    private async Task LoginGoogle() => await _navigator.NavigateRouteAsync(this, "Home");

    [RelayCommand]
    private async Task ForgotPassword() { ErrorMessage = "Reset link sent to your email."; await Task.CompletedTask; }

    [RelayCommand]
    private async Task GoRegister() => await _navigator.NavigateRouteAsync(this, "Register");
}

public partial class RegisterViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private string username = "";
    [ObservableProperty] private string email = "";
    [ObservableProperty] private string password = "";
    [ObservableProperty] private string? errorMessage;

    public RegisterViewModel(INavigator navigator, IChefsDataService data)
    { _navigator = navigator; _data = data; }

    [RelayCommand]
    private async Task SignUp()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        { ErrorMessage = "All fields are required"; return; }
        ErrorMessage = null;
        await _navigator.NavigateRouteAsync(this, "Home");
    }

    [RelayCommand]
    private async Task GoLogin() => await _navigator.NavigateBackAsync(this);
}
