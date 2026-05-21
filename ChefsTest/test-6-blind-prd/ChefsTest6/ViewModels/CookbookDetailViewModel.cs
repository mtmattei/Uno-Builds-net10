namespace ChefsTest6.ViewModels;

public partial class CookbookDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IRecipeService _recipes;

    public CookbookData Cookbook { get; }

    public CookbookDetailViewModel(INavigator navigator, IRecipeService recipes, CookbookData cookbook)
    {
        _navigator = navigator;
        _recipes = recipes;
        Cookbook = cookbook;
    }

    public string CountLabel => Cookbook.CountLabel;
    public bool HasRecipes => Cookbook.Recipes.Count > 0;

    [RelayCommand]
    private async Task OpenRecipe(RecipeData? r)
    {
        if (r is null) return;
        await _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: r);
    }

    [RelayCommand]
    private void ToggleFavorite(RecipeData? r)
    {
        if (r is null) return;
        _recipes.ToggleFavorite(r);
    }

    [RelayCommand]
    private async Task EditCookbook()
    {
        await _navigator.NavigateViewModelAsync<UpdateCookbookViewModel>(this, data: Cookbook);
    }
}
