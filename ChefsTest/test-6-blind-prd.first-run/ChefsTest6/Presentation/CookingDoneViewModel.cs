namespace ChefsTest6.Presentation;

public partial class CookingDoneViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefService _chef;

    public Recipe Recipe { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStar1), nameof(IsStar2), nameof(IsStar3),
        nameof(IsStar4), nameof(IsStar5))]
    private int rating;

    public bool IsStar1 => Rating >= 1;
    public bool IsStar2 => Rating >= 2;
    public bool IsStar3 => Rating >= 3;
    public bool IsStar4 => Rating >= 4;
    public bool IsStar5 => Rating >= 5;

    public bool IsFavorite => Recipe.IsFavorite;

    public CookingDoneViewModel(INavigator navigator, IChefService chef, Recipe recipe)
    {
        _navigator = navigator;
        _chef = chef;
        Recipe = recipe;
    }

    [RelayCommand] private void RateOne() => Rating = 1;
    [RelayCommand] private void RateTwo() => Rating = 2;
    [RelayCommand] private void RateThree() => Rating = 3;
    [RelayCommand] private void RateFour() => Rating = 4;
    [RelayCommand] private void RateFive() => Rating = 5;

    [RelayCommand]
    private void ToggleFavorite()
    {
        _chef.ToggleFavorite(Recipe);
        OnPropertyChanged(nameof(IsFavorite));
    }

    [RelayCommand]
    private async Task PreviousAsync() => await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);

    [RelayCommand]
    private async Task BackAsync() => await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);

    [RelayCommand]
    private async Task DoneAsync()
    {
        await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);
    }
}
