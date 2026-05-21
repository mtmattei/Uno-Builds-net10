using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest3.Presentation;

public partial class LiveCookingFinishViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty]
    private int _rating = 4;

    [ObservableProperty]
    private bool _isFavorite;

    public LiveCookingFinishViewModel(INavigator navigator)
    {
        _navigator = navigator;
    }

    [RelayCommand]
    private void SetRating(string value)
    {
        if (int.TryParse(value, out var v)) Rating = v;
    }

    [RelayCommand]
    private void ToggleFavorite() => IsFavorite = !IsFavorite;

    [RelayCommand]
    private async void Previous()
    {
        await _navigator.NavigateBackAsync(this);
    }

    [RelayCommand]
    private async void GoHome()
    {
        await _navigator.NavigateRouteAsync(this, "Home", qualifier: Qualifiers.ClearBackStack);
    }
}
