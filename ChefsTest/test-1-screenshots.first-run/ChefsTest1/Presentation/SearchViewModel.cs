using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ChefsTest1.Models;
using ChefsTest1.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class SearchViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;
    private List<RecipeData> _all = new();

    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private ObservableCollection<RecipeData> results = new();
    [ObservableProperty] private string resultCount = "0 results";
    [ObservableProperty] private bool isEmpty;

    public SearchViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        _ = Load();
    }

    private async Task Load()
    {
        _all = (await _data.GetRecipesAsync()).ToList();
        Apply();
    }

    partial void OnSearchTextChanged(string value) => Apply();

    private void Apply()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _all
            : _all.Where(r => (r.Name ?? "").Contains(SearchText, System.StringComparison.OrdinalIgnoreCase)).ToList();
        Results = new ObservableCollection<RecipeData>(filtered);
        ResultCount = filtered.Count == 1 ? "1 result" : $"{filtered.Count} results";
        IsEmpty = filtered.Count == 0;
    }

    [RelayCommand]
    private Task GoHome() => _navigator.NavigateViewModelAsync<HomeViewModel>(this, qualifier: Qualifiers.ClearBackStack);

    [RelayCommand]
    private Task GoFavorites() => _navigator.NavigateViewModelAsync<FavoritesViewModel>(this);

    [RelayCommand]
    private Task OpenFilters() => _navigator.NavigateViewModelAsync<FiltersViewModel>(this);

    [RelayCommand]
    private Task OpenRecipe(RecipeData? r) =>
        r == null ? Task.CompletedTask : _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: r);
}
