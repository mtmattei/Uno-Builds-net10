using System.Collections.ObjectModel;

namespace ChefsTest6.Presentation;

public partial class CookbookDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefService _chef;

    public Cookbook Cookbook { get; }

    public ObservableCollection<Recipe> Recipes { get; } = new();

    public string CountText => $"{Cookbook.Recipes.Count} {(Cookbook.Recipes.Count == 1 ? "recipe" : "recipes")}";

    public CookbookDetailViewModel(INavigator navigator, IChefService chef, Cookbook cookbook)
    {
        _navigator = navigator;
        _chef = chef;
        Cookbook = cookbook;
        foreach (var r in cookbook.Recipes) Recipes.Add(r);
    }

    [RelayCommand]
    private async Task OpenRecipeAsync(Recipe? recipe)
    {
        if (recipe is null) return;
        await _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: recipe);
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        await _navigator.NavigateViewModelAsync<EditCookbookViewModel>(this, data: Cookbook);
    }

    [RelayCommand]
    private void ToggleFavorite(Recipe? recipe)
    {
        if (recipe is null) return;
        _chef.ToggleFavorite(recipe);
    }

    [RelayCommand]
    private async Task BackAsync() => await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);
}
