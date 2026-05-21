using System.Collections.ObjectModel;
using ChefsTest3.Models;
using ChefsTest3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest3.Presentation;

public partial class FavoritesAllRecipesViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsService _service;

    public ObservableCollection<RecipeData> Recipes { get; } = new();

    [ObservableProperty]
    private bool _hasItems = true;

    public FavoritesAllRecipesViewModel(INavigator navigator, IChefsService service)
    {
        _navigator = navigator;
        _service = service;
        _ = LoadAsync();
    }

    private async System.Threading.Tasks.Task LoadAsync()
    {
        var favorites = await _service.GetFavoriteRecipesAsync();
        Recipes.Clear();
        foreach (var r in favorites) Recipes.Add(r);
        HasItems = Recipes.Count > 0;
    }

    [RelayCommand]
    private async void OpenRecipe(RecipeData recipe)
    {
        if (recipe is null) return;
        await _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: recipe);
    }

    [RelayCommand]
    private async void GoToCookbooks()
    {
        await _navigator.NavigateRouteAsync(this, "FavoritesCookbooks", qualifier: Qualifiers.Root);
    }

    [RelayCommand]
    private async void OpenHome()
    {
        await _navigator.NavigateRouteAsync(this, "Home", qualifier: Qualifiers.Root);
    }

    [RelayCommand]
    private async void OpenSearch()
    {
        await _navigator.NavigateRouteAsync(this, "Search", qualifier: Qualifiers.Root);
    }
}
