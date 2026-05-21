using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChefsTest1.Models;
using ChefsTest1.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class HomeViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private ObservableCollection<RecipeData> trending = new();
    [ObservableProperty] private ObservableCollection<CategoryData> categories = new();
    [ObservableProperty] private ObservableCollection<RecipeData> recent = new();
    [ObservableProperty] private ObservableCollection<UserData> creators = new();
    [ObservableProperty] private string diagnostic = "loading...";

    public HomeViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        _ = Load();
    }

    private async Task Load()
    {
        var t = await _data.GetTrendingAsync();
        Trending = new ObservableCollection<RecipeData>(t);
        var c = await _data.GetCategoriesAsync();
        Categories = new ObservableCollection<CategoryData>(c);
        var r = await _data.GetRecentlyAddedAsync();
        Recent = new ObservableCollection<RecipeData>(r);
        var u = await _data.GetPopularCreatorsAsync();
        Creators = new ObservableCollection<UserData>(u);
        Diagnostic = $"R:{t.Count} C:{c.Count} U:{u.Count} | err:{ChefsDataService.LastError ?? "ok"}";
    }

    [RelayCommand]
    private Task GoSearch() => _navigator.NavigateViewModelAsync<SearchViewModel>(this);

    [RelayCommand]
    private Task GoFavorites() => _navigator.NavigateViewModelAsync<FavoritesViewModel>(this);

    [RelayCommand]
    private Task OpenProfile() => _navigator.NavigateViewModelAsync<ProfileViewModel>(this);

    [RelayCommand]
    private Task OpenNotifications() => _navigator.NavigateViewModelAsync<NotificationsViewModel>(this);

    [RelayCommand]
    private Task OpenNearMe() => _navigator.NavigateViewModelAsync<NearMeMapViewModel>(this);

    [RelayCommand]
    private Task OpenRecipe(RecipeData? r) =>
        r == null ? Task.CompletedTask : _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: r);

    [RelayCommand]
    private async Task ToggleFavorite(RecipeData? r)
    {
        if (r == null) return;
        await _data.ToggleFavoriteAsync(r.Id);
        var idx = Trending.IndexOf(r);
        if (idx >= 0)
        {
            Trending.RemoveAt(idx);
            Trending.Insert(idx, r);
        }
    }
}
