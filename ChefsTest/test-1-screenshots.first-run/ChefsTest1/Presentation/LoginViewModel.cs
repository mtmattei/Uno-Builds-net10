using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class LoginViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty]
    private string? username;

    [ObservableProperty]
    private string? password;

    [ObservableProperty]
    private bool rememberMe;

    public LoginViewModel(INavigator navigator)
    {
        _navigator = navigator;
    }

    [RelayCommand]
    private Task Login() => GoHome();

    [RelayCommand]
    private Task LoginApple() => GoHome();

    [RelayCommand]
    private Task LoginGoogle() => GoHome();

    [RelayCommand]
    private Task GoToRegister() => _navigator.NavigateViewModelAsync<RegisterViewModel>(this);

    private Task GoHome() => _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);
}
