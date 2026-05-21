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

public partial class FavoritesViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private ObservableCollection<RecipeData> recipes = new();
    [ObservableProperty] private ObservableCollection<CookbookData> cookbooks = new();
    [ObservableProperty] private bool isAllRecipesTab = true;
    [ObservableProperty] private bool isCookbooksTab;
    [ObservableProperty] private string resultCount = "0 results";
    [ObservableProperty] private bool hasRecipes;
    [ObservableProperty] private bool hasCookbooks;

    public FavoritesViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        _ = Load();
    }

    private async Task Load()
    {
        var fav = await _data.GetFavoritedAsync();
        Recipes = new ObservableCollection<RecipeData>(fav);
        HasRecipes = fav.Count > 0;
        ResultCount = $"{fav.Count} results";

        var cb = await _data.GetSavedCookbooksAsync();
        Cookbooks = new ObservableCollection<CookbookData>(cb);
        HasCookbooks = cb.Count > 0;
    }

    [RelayCommand]
    private void ShowAllRecipes()
    {
        IsAllRecipesTab = true;
        IsCookbooksTab = false;
        ResultCount = $"{Recipes.Count} results";
    }

    [RelayCommand]
    private void ShowCookbooks()
    {
        IsAllRecipesTab = false;
        IsCookbooksTab = true;
        ResultCount = $"{Cookbooks.Count} results";
    }

    [RelayCommand]
    private Task GoHome() => _navigator.NavigateViewModelAsync<HomeViewModel>(this, qualifier: Qualifiers.ClearBackStack);

    [RelayCommand]
    private Task GoSearch() => _navigator.NavigateViewModelAsync<SearchViewModel>(this);

    [RelayCommand]
    private Task OpenProfile() => _navigator.NavigateViewModelAsync<ProfileViewModel>(this);

    [RelayCommand]
    private Task OpenNotifications() => _navigator.NavigateViewModelAsync<NotificationsViewModel>(this);

    [RelayCommand]
    private Task OpenRecipe(RecipeData? r) =>
        r == null ? Task.CompletedTask : _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: r);

    [RelayCommand]
    private Task OpenCookbook(CookbookData? c) =>
        c == null ? Task.CompletedTask : _navigator.NavigateViewModelAsync<CookbookDetailViewModel>(this, data: c);

    [RelayCommand]
    private Task CreateCookbook() => _navigator.NavigateViewModelAsync<CreateCookbookViewModel>(this);
}
