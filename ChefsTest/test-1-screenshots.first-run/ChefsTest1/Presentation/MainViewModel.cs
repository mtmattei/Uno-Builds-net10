using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class MainViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public MainViewModel(INavigator navigator)
    {
        _navigator = navigator;
        _ = GoHome();
    }

    private async Task GoHome()
    {
        await Task.Delay(1);
        await _navigator.NavigateViewModelAsync<HomeViewModel>(this, qualifier: Qualifiers.ClearBackStack);
    }
}
