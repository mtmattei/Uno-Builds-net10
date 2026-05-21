using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class RegisterViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty] private string? username;
    [ObservableProperty] private string? email;
    [ObservableProperty] private string? password;

    public RegisterViewModel(INavigator navigator)
    {
        _navigator = navigator;
    }

    [RelayCommand]
    private Task SignUp() => _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);

    [RelayCommand]
    private Task BackToLogin() => _navigator.NavigateBackAsync(this);
}
