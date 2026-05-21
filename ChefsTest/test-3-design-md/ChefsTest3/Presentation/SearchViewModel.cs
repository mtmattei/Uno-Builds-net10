using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChefsTest3.Models;
using ChefsTest3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest3.Presentation;

public partial class SearchViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsService _service;

    [ObservableProperty]
    private string _query = string.Empty;

    [ObservableProperty]
    private int _resultCount;

    [ObservableProperty]
    private bool _hasResults = true;

    public ObservableCollection<RecipeData> Results { get; } = new();

    public SearchViewModel(INavigator navigator, IChefsService service)
    {
        _navigator = navigator;
        _service = service;
        _ = LoadAsync();
    }

    public SearchViewModel(INavigator navigator, IChefsService service, string preset)
        : this(navigator, service)
    {
        Query = preset;
    }

    partial void OnQueryChanged(string value)
    {
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var results = await _service.SearchRecipesAsync(Query);
        Results.Clear();
        foreach (var r in results) Results.Add(r);
        ResultCount = Results.Count;
        HasResults = ResultCount > 0;
    }

    [RelayCommand]
    private async void OpenRecipe(RecipeData recipe)
    {
        if (recipe is null) return;
        await _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: recipe);
    }

    [RelayCommand]
    private async void OpenFilters()
    {
        await _navigator.NavigateRouteAsync(this, "Filters");
    }

    [RelayCommand]
    private async void OpenHome()
    {
        await _navigator.NavigateRouteAsync(this, "Home", qualifier: Qualifiers.Root);
    }

    [RelayCommand]
    private async void OpenFavorites()
    {
        await _navigator.NavigateRouteAsync(this, "FavoritesAll", qualifier: Qualifiers.Root);
    }
}
