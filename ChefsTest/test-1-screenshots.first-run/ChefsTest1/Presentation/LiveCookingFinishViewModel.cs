using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class LiveCookingFinishViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty] private int rating;

    public LiveCookingFinishViewModel(INavigator navigator)
    {
        _navigator = navigator;
    }

    [RelayCommand]
    private void RateOne() => Rating = 1;
    [RelayCommand]
    private void RateTwo() => Rating = 2;
    [RelayCommand]
    private void RateThree() => Rating = 3;
    [RelayCommand]
    private void RateFour() => Rating = 4;
    [RelayCommand]
    private void RateFive() => Rating = 5;

    [RelayCommand]
    private Task Previous() => _navigator.NavigateBackAsync(this);

    [RelayCommand]
    private Task Favorite() => _navigator.NavigateBackAsync(this);

    [RelayCommand]
    private Task Back() => _navigator.NavigateBackAsync(this);
}
