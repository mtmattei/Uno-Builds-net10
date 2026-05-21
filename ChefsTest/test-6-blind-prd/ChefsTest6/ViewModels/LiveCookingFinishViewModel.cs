namespace ChefsTest6.ViewModels;

public partial class LiveCookingFinishViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IRecipeService _recipes;

    public RecipeData Recipe { get; }

    [ObservableProperty]
    private int rating;

    public LiveCookingFinishViewModel(INavigator navigator, IRecipeService recipes, RecipeData recipe)
    {
        _navigator = navigator;
        _recipes = recipes;
        Recipe = recipe;
    }

    [RelayCommand]
    private void SetRating(string? value)
    {
        if (int.TryParse(value, out var stars)) Rating = stars;
    }

    [RelayCommand]
    private async Task Previous() => await _navigator.NavigateBackAsync(this);

    [RelayCommand]
    private void ToggleFavorite() => _recipes.ToggleFavorite(Recipe);
}
