using System.Collections.ObjectModel;

namespace ChefsTest6.ViewModels;

public partial class SearchViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IRecipeService _recipes;

    [ObservableProperty]
    private string query = string.Empty;

    [ObservableProperty]
    private CategoryData? selectedCategory;

    [ObservableProperty]
    private int? maxMinutes;

    [ObservableProperty]
    private int? selectedDifficulty;

    public ObservableCollection<RecipeData> Results { get; } = new();

    public IReadOnlyList<CategoryData> AllCategories { get; }

    public SearchViewModel(INavigator navigator, IRecipeService recipes)
    {
        _navigator = navigator;
        _recipes = recipes;
        AllCategories = recipes.Categories();
        ApplyFilters();
    }

    public SearchViewModel(INavigator navigator, IRecipeService recipes, CategoryData category)
        : this(navigator, recipes)
    {
        SelectedCategory = category;
        ApplyFilters();
    }

    public SearchViewModel(INavigator navigator, IRecipeService recipes, string preset)
        : this(navigator, recipes)
    {
        if (preset == "trending")
        {
            Results.Clear();
            foreach (var r in recipes.Trending()) Results.Add(r);
        }
        else if (preset == "recent")
        {
            Results.Clear();
            foreach (var r in recipes.RecentlyAdded()) Results.Add(r);
        }
    }

    public bool HasFilters => SelectedCategory is not null || MaxMinutes is not null || SelectedDifficulty is not null;
    public bool HasResults => Results.Count > 0;
    public string ResultCountLabel => Results.Count switch
    {
        0 => "No results",
        1 => "1 result",
        _ => $"{Results.Count} results"
    };

    partial void OnQueryChanged(string value) => ApplyFilters();

    public void ApplyFilters()
    {
        Results.Clear();
        foreach (var r in _recipes.Search(Query, SelectedCategory, MaxMinutes, SelectedDifficulty))
        {
            Results.Add(r);
        }
        OnPropertyChanged(nameof(HasFilters));
        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(ResultCountLabel));
    }

    [RelayCommand]
    private async Task OpenFilters()
    {
        var ctx = new FiltersContext
        {
            Category = SelectedCategory,
            MaxMinutes = MaxMinutes,
            Difficulty = SelectedDifficulty,
            AllCategories = AllCategories,
        };
        await _navigator.NavigateViewModelAsync<FiltersViewModel>(this, data: ctx);
    }

    [RelayCommand]
    private async Task OpenRecipe(RecipeData? recipe)
    {
        if (recipe is null) return;
        await _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: recipe);
    }

    [RelayCommand]
    private void ToggleFavorite(RecipeData? recipe)
    {
        if (recipe is null) return;
        _recipes.ToggleFavorite(recipe);
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SelectedCategory = null;
        MaxMinutes = null;
        SelectedDifficulty = null;
        ApplyFilters();
    }

    public void UpdateFromContext(FiltersContext ctx)
    {
        SelectedCategory = ctx.Category;
        MaxMinutes = ctx.MaxMinutes;
        SelectedDifficulty = ctx.Difficulty;
        ApplyFilters();
    }
}

public class FiltersContext
{
    public CategoryData? Category { get; set; }
    public int? MaxMinutes { get; set; }
    public int? Difficulty { get; set; }
    public IReadOnlyList<CategoryData> AllCategories { get; set; } = Array.Empty<CategoryData>();
}
