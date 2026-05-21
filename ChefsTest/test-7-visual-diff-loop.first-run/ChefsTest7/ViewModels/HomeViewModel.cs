using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChefsTest7.Models;
using ChefsTest7.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest7.ViewModels;

public partial class HomeViewModel : TabHostViewModelBase
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    public ObservableCollection<RecipeData> Trending { get; } = new();
    public ObservableCollection<CategoryData> Categories { get; } = new();
    public ObservableCollection<RecipeData> RecentlyAdded { get; } = new();
    public ObservableCollection<UserData> PopularContributors { get; } = new();

    [ObservableProperty] private string greeting = "Hello, Jamie";

    public HomeViewModel(INavigator navigator, IChefsDataService data) : base(navigator)
    {
        _navigator = navigator; _data = data;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var trending = await _data.GetTrendingAsync();
        foreach (var r in trending) Trending.Add(r);
        var cats = await _data.GetCategoriesAsync();
        foreach (var c in cats) Categories.Add(c);
        var recent = await _data.GetPopularAsync();
        foreach (var r in recent) RecentlyAdded.Add(r);
        var users = await _data.GetPopularCreatorsAsync();
        foreach (var u in users) PopularContributors.Add(u);
        var me = await _data.GetCurrentUserAsync();
        if (me is not null) Greeting = $"Hello, {me.FullName?.Split(' ').FirstOrDefault()}";
    }

    [RelayCommand]
    private async Task OpenRecipe(RecipeData? r)
    {
        if (r is null) return;
        await _navigator.NavigateRouteAsync(this, "RecipeDetail", data: r);
    }

    [RelayCommand]
    private async Task ToggleFavorite(RecipeData? r)
    {
        if (r is null) return;
        await _data.ToggleFavoriteAsync(r.Id);
        r.IsFavorite = !r.IsFavorite;
        OnPropertyChanged(nameof(Trending));
        OnPropertyChanged(nameof(RecentlyAdded));
    }

    [RelayCommand]
    private async Task OpenSearch() => await _navigator.NavigateRouteAsync(this, "Search");

    [RelayCommand]
    private async Task OpenNearMe() => await _navigator.NavigateRouteAsync(this, "NearMeMap");

    [RelayCommand]
    private async Task OpenProfile() => await _navigator.NavigateRouteAsync(this, "Profile");

    [RelayCommand]
    private async Task OpenContributor(UserData? u)
    {
        if (u is null) return;
        await _navigator.NavigateRouteAsync(this, "OtherProfile", data: u);
    }

    [RelayCommand]
    private async Task OpenNotifications() => await _navigator.NavigateRouteAsync(this, "Notifications");
}
