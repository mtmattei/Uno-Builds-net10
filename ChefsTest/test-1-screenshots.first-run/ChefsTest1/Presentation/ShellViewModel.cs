using System.Threading.Tasks;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class ShellViewModel
{
    private readonly INavigator _navigator;

    public ShellViewModel(INavigator navigator)
    {
        _navigator = navigator;
        _ = Start();
    }

    private async Task Start()
    {
        await Task.Delay(400);
        await _navigator.NavigateViewModelAsync<OnboardingViewModel>(this);
    }
}
