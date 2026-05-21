using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest3.Presentation;

public partial class RegisterViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    public RegisterViewModel(INavigator navigator)
    {
        _navigator = navigator;
    }

    [RelayCommand]
    private async void SignUp()
    {
        await _navigator.NavigateRouteAsync(this, "Home", qualifier: Qualifiers.ClearBackStack);
    }

    [RelayCommand]
    private async void GoToLogin()
    {
        await _navigator.NavigateBackAsync(this);
    }
}
