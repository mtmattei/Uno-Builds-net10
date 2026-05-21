using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class OnboardingViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty]
    private int currentIndex;

    public OnboardingViewModel(INavigator navigator)
    {
        _navigator = navigator;
    }

    [RelayCommand]
    private void Next()
    {
        if (CurrentIndex < 2) CurrentIndex++;
        else _ = SkipExecute();
    }

    [RelayCommand]
    private void Previous()
    {
        if (CurrentIndex > 0) CurrentIndex--;
    }

    [RelayCommand]
    private Task Skip() => SkipExecute();

    private async Task SkipExecute()
    {
        await _navigator.NavigateViewModelAsync<LoginViewModel>(this, qualifier: Qualifiers.ClearBackStack);
    }
}
