using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest3.Presentation;

public partial class LoginViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _rememberMe;

    public LoginViewModel(INavigator navigator)
    {
        _navigator = navigator;
    }

    [RelayCommand]
    private async void Login()
    {
        await _navigator.NavigateRouteAsync(this, "Home", qualifier: Qualifiers.ClearBackStack);
    }

    [RelayCommand]
    private async void Register()
    {
        await _navigator.NavigateRouteAsync(this, "Register");
    }

    [RelayCommand]
    private async void Apple()
    {
        await _navigator.NavigateRouteAsync(this, "Home", qualifier: Qualifiers.ClearBackStack);
    }

    [RelayCommand]
    private async void Google()
    {
        await _navigator.NavigateRouteAsync(this, "Home", qualifier: Qualifiers.ClearBackStack);
    }

    [RelayCommand]
    private void ForgotPassword()
    {
        // No-op stub: surface a message in a real app.
    }
}
