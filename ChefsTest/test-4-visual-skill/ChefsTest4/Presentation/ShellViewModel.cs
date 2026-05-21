using System.Threading.Tasks;
using Uno.Extensions.Navigation;

namespace ChefsTest4.Presentation;

public class ShellViewModel
{
    private readonly INavigator _navigator;

    public ShellViewModel(INavigator navigator)
    {
        _navigator = navigator;
        _ = StartAsync();
    }

    private async Task StartAsync()
    {
        await Task.Delay(50);
        await _navigator.NavigateRouteAsync(this, "Login");
    }
}
