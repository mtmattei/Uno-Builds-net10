using System.Collections.ObjectModel;

namespace ChefsTest6.ViewModels;

public partial class FavoritesViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IRecipeService _recipes;
    private readonly ICookbookService _cookbooks;

    public ObservableCollection<RecipeData> SavedRecipes { get; } = new();
    public ObservableCollection<CookbookData> Cookbooks { get; }

    [ObservableProperty]
    private int selectedSegment;

    public FavoritesViewModel(INavigator navigator, IRecipeService recipes, ICookbookService cookbooks)
    {
        _navigator = navigator;
        _recipes = recipes;
        _cookbooks = cookbooks;
        foreach (var r in recipes.Favorites()) SavedRecipes.Add(r);
        Cookbooks = cookbooks.All();
    }

    public bool ShowAllRecipes => SelectedSegment == 0;
    public bool ShowCookbooks => SelectedSegment == 1;
    public bool HasSavedRecipes => SavedRecipes.Count > 0;
    public bool HasCookbooks => Cookbooks.Count > 0;
    public string SavedCountLabel => SavedRecipes.Count switch
    {
        0 => "No recipes saved",
        1 => "1 recipe",
        _ => $"{SavedRecipes.Count} recipes"
    };
    public string CookbookCountLabel => Cookbooks.Count switch
    {
        0 => "No cookbooks",
        1 => "1 cookbook",
        _ => $"{Cookbooks.Count} cookbooks"
    };

    partial void OnSelectedSegmentChanged(int value)
    {
        OnPropertyChanged(nameof(ShowAllRecipes));
        OnPropertyChanged(nameof(ShowCookbooks));
    }

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
        SavedRecipes.Clear();
        foreach (var x in _recipes.Favorites()) SavedRecipes.Add(x);
        OnPropertyChanged(nameof(HasSavedRecipes));
        OnPropertyChanged(nameof(SavedCountLabel));
    }

    [RelayCommand]
    private async Task OpenCookbook(CookbookData? c)
    {
        if (c is null) return;
        await _navigator.NavigateViewModelAsync<CookbookDetailViewModel>(this, data: c);
    }

    [RelayCommand]
    private async Task CreateCookbook()
    {
        await _navigator.NavigateViewModelAsync<CreateCookbookViewModel>(this);
    }

    [RelayCommand]
    private async Task OpenSearch()
    {
        await _navigator.NavigateViewModelAsync<SearchViewModel>(this);
    }

    [RelayCommand]
    private void SelectSegment(string? value)
    {
        if (int.TryParse(value, out var i)) SelectedSegment = i;
    }
}
