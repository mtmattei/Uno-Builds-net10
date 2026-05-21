using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChefsTest7.Models;
using ChefsTest7.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest7.ViewModels;

public partial class SearchViewModel : TabHostViewModelBase
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;
    private List<RecipeData> _all = new();

    [ObservableProperty] private string query = string.Empty;
    public ObservableCollection<RecipeData> Results { get; } = new();
    public int ResultCount => Results.Count;
    public bool HasResults => Results.Count > 0;

    public SearchViewModel(INavigator navigator, IChefsDataService data) : base(navigator)
    { _navigator = navigator; _data = data; _ = LoadAsync(); }

    partial void OnQueryChanged(string value) => ApplyFilter();

    private async Task LoadAsync()
    {
        _all = (await _data.GetRecipesAsync()).ToList();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        Results.Clear();
        var q = (Query ?? string.Empty).Trim();
        var src = string.IsNullOrEmpty(q) ? _all : _all.Where(r => (r.Name ?? "").Contains(q, StringComparison.OrdinalIgnoreCase) || (r.Category?.Name ?? "").Contains(q, StringComparison.OrdinalIgnoreCase));
        foreach (var r in src) Results.Add(r);
        OnPropertyChanged(nameof(ResultCount));
        OnPropertyChanged(nameof(HasResults));
    }

    [RelayCommand]
    private async Task OpenFilters() => await _navigator.NavigateRouteAsync(this, "Filters");

    [RelayCommand]
    private async Task OpenRecipe(RecipeData? r)
    { if (r is not null) await _navigator.NavigateRouteAsync(this, "RecipeDetail", data: r); }

    [RelayCommand]
    private async Task ToggleFavorite(RecipeData? r)
    {
        if (r is null) return;
        await _data.ToggleFavoriteAsync(r.Id);
        r.IsFavorite = !r.IsFavorite;
    }

    [RelayCommand]
    private void ClearQuery() => Query = string.Empty;
}

public partial class FiltersViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    public ObservableCollection<CategoryData> Categories { get; } = new();
    public ObservableCollection<string> Times { get; } = new() { "< 15 min", "15–30 min", "30–60 min", "> 60 min" };
    public ObservableCollection<string> Levels { get; } = new() { "Easy", "Medium", "Hard" };

    [ObservableProperty] private CategoryData? selectedCategory;
    [ObservableProperty] private string? selectedTime;
    [ObservableProperty] private string? selectedLevel;

    public FiltersViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator; _data = data;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var cats = await _data.GetCategoriesAsync();
        foreach (var c in cats) Categories.Add(c);
    }

    [RelayCommand]
    private void Reset()
    {
        SelectedCategory = null; SelectedTime = null; SelectedLevel = null;
    }

    [RelayCommand]
    private async Task Apply() => await _navigator.NavigateBackAsync(this);

    [RelayCommand]
    private async Task Close() => await _navigator.NavigateBackAsync(this);
}
