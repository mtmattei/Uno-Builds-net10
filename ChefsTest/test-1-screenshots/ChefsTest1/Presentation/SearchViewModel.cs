using System.Collections.ObjectModel;

namespace ChefsTest1.Presentation;

public partial class SearchViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IRecipeService _recipes;
    private List<Recipe> _all = new();

    public ObservableCollection<Recipe> Results { get; } = new();

    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private bool hasResults = true;

    public string ResultCount => Results.Count == 1 ? "1 result" : $"{Results.Count} results";

    public SearchViewModel(INavigator navigator, IRecipeService recipes)
    {
        _navigator = navigator;
        _recipes = recipes;
        OpenRecipeCommand = new AsyncRelayCommand<Recipe>(OpenRecipeAsync!);
        OpenFiltersCommand = new AsyncRelayCommand(OpenFiltersAsync);
        OpenPopularCommand = new RelayCommand(() => { SearchText = string.Empty; });
        OpenNotificationsCommand = new AsyncRelayCommand(OpenNotificationsAsync);
        OpenOwnProfileCommand = new AsyncRelayCommand(OpenOwnProfileAsync);
        _ = LoadAsync();
    }

    public ICommand OpenRecipeCommand { get; }
    public ICommand OpenFiltersCommand { get; }
    public ICommand OpenPopularCommand { get; }
    public ICommand OpenNotificationsCommand { get; }
    public ICommand OpenOwnProfileCommand { get; }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    private async Task LoadAsync()
    {
        var all = await _recipes.GetAllAsync();
        _all = all.ToList();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        Results.Clear();
        var q = (SearchText ?? string.Empty).Trim();
        var filtered = string.IsNullOrEmpty(q)
            ? _all
            : _all.Where(r => (r.Name ?? string.Empty).IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        foreach (var r in filtered) Results.Add(r);
        HasResults = Results.Count > 0;
        OnPropertyChanged(nameof(ResultCount));
    }

    private async Task OpenRecipeAsync(Recipe recipe)
    {
        if (recipe == null) return;
        await _navigator.NavigateRouteAsync(this, "RecipeDetail", data: recipe);
    }

    private async Task OpenFiltersAsync()
    {
        await _navigator.NavigateRouteAsync(this, "Filters");
    }

    private async Task OpenNotificationsAsync()
    {
        await _navigator.NavigateRouteAsync(this, "Notifications");
    }

    private async Task OpenOwnProfileAsync()
    {
        await _navigator.NavigateRouteAsync(this, "Profile");
    }
}
