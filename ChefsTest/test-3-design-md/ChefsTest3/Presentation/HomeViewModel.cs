using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ChefsTest3.Models;
using ChefsTest3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest3.Presentation;

public partial class HomeViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsService _service;

    [ObservableProperty]
    private string _greeting = "Good morning";

    [ObservableProperty]
    private string _userName = "Andrea";

    public ObservableCollection<RecipeData> TrendingRecipes { get; } = new();
    public ObservableCollection<RecipeData> RecentlyAdded { get; } = new();
    public ObservableCollection<CategoryData> Categories { get; } = new();
    public ObservableCollection<UserData> PopularContributors { get; } = new();

    public HomeViewModel(INavigator navigator, IChefsService service)
    {
        _navigator = navigator;
        _service = service;
        SetGreeting();
        _ = LoadAsync();
    }

    private void SetGreeting()
    {
        var hour = DateTime.Now.Hour;
        Greeting = hour switch
        {
            < 12 => "Good morning",
            < 18 => "Good afternoon",
            _ => "Good evening",
        };
    }

    private async Task LoadAsync()
    {
        var trending = await _service.GetTrendingAsync();
        TrendingRecipes.Clear();
        foreach (var r in trending) TrendingRecipes.Add(r);

        var recent = await _service.GetRecentlyAddedAsync();
        RecentlyAdded.Clear();
        foreach (var r in recent) RecentlyAdded.Add(r);

        var cats = await _service.GetCategoriesAsync();
        Categories.Clear();
        foreach (var c in cats) Categories.Add(c);

        var creators = await _service.GetPopularCreatorsAsync();
        PopularContributors.Clear();
        foreach (var u in creators) PopularContributors.Add(u);

        var user = await _service.GetCurrentUserAsync();
        if (!string.IsNullOrEmpty(user.FullName))
        {
            UserName = user.FullName.Split(' ').FirstOrDefault() ?? "Chef";
        }
    }

    [RelayCommand]
    private async void OpenRecipe(RecipeData recipe)
    {
        if (recipe is null) return;
        await _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: recipe);
    }

    [RelayCommand]
    private async void OpenContributor(UserData user)
    {
        if (user is null) return;
        await _navigator.NavigateViewModelAsync<OtherProfileViewModel>(this, data: user);
    }

    [RelayCommand]
    private async void OpenCategory(CategoryData category)
    {
        if (category is null) return;
        await _navigator.NavigateRouteAsync(this, "Search", data: category.Name);
    }

    [RelayCommand]
    private async void OpenSearch()
    {
        await _navigator.NavigateRouteAsync(this, "Search", qualifier: Qualifiers.Root);
    }

    [RelayCommand]
    private async void OpenFavorites()
    {
        await _navigator.NavigateRouteAsync(this, "FavoritesAll", qualifier: Qualifiers.Root);
    }

    [RelayCommand]
    private async void OpenNotifications()
    {
        await _navigator.NavigateRouteAsync(this, "Notifications");
    }

    [RelayCommand]
    private async void OpenNearMe()
    {
        await _navigator.NavigateRouteAsync(this, "NearMeMap");
    }

    [RelayCommand]
    private async void OpenProfile()
    {
        await _navigator.NavigateRouteAsync(this, "Profile");
    }

    [RelayCommand]
    private async void OpenSettings()
    {
        await _navigator.NavigateRouteAsync(this, "Settings");
    }

    [RelayCommand]
    private async void ToggleFavorite(RecipeData recipe)
    {
        if (recipe is null) return;
        await _service.ToggleFavoriteRecipeAsync(recipe.Id);
        // refresh in-place
        var idx = TrendingRecipes.IndexOf(recipe);
        if (idx >= 0)
        {
            TrendingRecipes.RemoveAt(idx);
            TrendingRecipes.Insert(idx, recipe);
        }
        var idx2 = RecentlyAdded.IndexOf(recipe);
        if (idx2 >= 0)
        {
            RecentlyAdded.RemoveAt(idx2);
            RecentlyAdded.Insert(idx2, recipe);
        }
    }
}
