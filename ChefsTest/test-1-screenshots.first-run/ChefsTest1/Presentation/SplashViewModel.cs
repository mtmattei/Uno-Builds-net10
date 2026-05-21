using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class SplashViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public SplashViewModel(INavigator navigator)
    {
        _navigator = navigator;
        _ = Continue();
    }

    private async Task Continue()
    {
        await Task.Delay(700);
        await _navigator.NavigateViewModelAsync<OnboardingViewModel>(this, qualifier: Qualifiers.ClearBackStack);
    }
}
