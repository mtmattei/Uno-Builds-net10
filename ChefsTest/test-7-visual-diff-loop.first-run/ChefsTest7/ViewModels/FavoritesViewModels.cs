using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChefsTest7.Models;
using ChefsTest7.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest7.ViewModels;

public partial class FavoritesAllRecipesViewModel : TabHostViewModelBase
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;
    public ObservableCollection<RecipeData> Items { get; } = new();
    public bool HasItems => Items.Count > 0;

    public FavoritesAllRecipesViewModel(INavigator navigator, IChefsDataService data) : base(navigator)
    { _navigator = navigator; _data = data; _ = LoadAsync(); }

    private async Task LoadAsync()
    {
        var items = await _data.GetFavoritedAsync();
        foreach (var r in items) Items.Add(r);
        OnPropertyChanged(nameof(HasItems));
    }

    [RelayCommand]
    private async Task OpenRecipe(RecipeData? r)
    { if (r is not null) await _navigator.NavigateRouteAsync(this, "RecipeDetail", data: r); }

    [RelayCommand]
    private async Task GoMyCookbooks() => await _navigator.NavigateRouteAsync(this, "FavoritesMyCookbooks");

    [RelayCommand]
    private async Task SeePopular() => await _navigator.NavigateRouteAsync(this, "Home");
}

public partial class FavoritesMyCookbooksViewModel : TabHostViewModelBase
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;
    public ObservableCollection<CookbookData> Items { get; } = new();
    public bool HasItems => Items.Count > 0;

    public FavoritesMyCookbooksViewModel(INavigator navigator, IChefsDataService data) : base(navigator)
    { _navigator = navigator; _data = data; _ = LoadAsync(); }

    private async Task LoadAsync()
    {
        var items = await _data.GetCookbooksAsync();
        foreach (var c in items) Items.Add(c);
        OnPropertyChanged(nameof(HasItems));
    }

    [RelayCommand]
    private async Task OpenCookbook(CookbookData? c)
    { if (c is not null) await _navigator.NavigateRouteAsync(this, "CookbookDetail", data: c); }

    [RelayCommand]
    private async Task CreateCookbook() => await _navigator.NavigateRouteAsync(this, "CreateCookbook");

    [RelayCommand]
    private async Task GoAllRecipes() => await _navigator.NavigateRouteAsync(this, "FavoritesAllRecipes");
}

public partial class CookbookDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;
    [ObservableProperty] private CookbookData? cookbook;
    public ObservableCollection<RecipeData> Recipes { get; } = new();
    public int RecipeCount => Recipes.Count;

    public CookbookDetailViewModel(INavigator navigator, IChefsDataService data, CookbookData? cookbook = null)
    {
        _navigator = navigator; _data = data;
        Cookbook = cookbook;
        foreach (var r in cookbook?.Recipes ?? Enumerable.Empty<RecipeData>()) Recipes.Add(r);
        OnPropertyChanged(nameof(RecipeCount));
    }

    [RelayCommand]
    private async Task OpenRecipe(RecipeData? r)
    { if (r is not null) await _navigator.NavigateRouteAsync(this, "RecipeDetail", data: r); }

    [RelayCommand]
    private async Task Edit() { if (Cookbook is not null) await _navigator.NavigateRouteAsync(this, "UpdateCookbook", data: Cookbook); }

    [RelayCommand]
    private async Task Back() => await _navigator.NavigateBackAsync(this);
}

public partial class CreateCookbookViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;
    [ObservableProperty] private string name = string.Empty;
    public ObservableCollection<RecipeData> Available { get; } = new();
    public ObservableCollection<RecipeData> Selected { get; } = new();

    public CreateCookbookViewModel(INavigator navigator, IChefsDataService data)
    { _navigator = navigator; _data = data; _ = LoadAsync(); }

    private async Task LoadAsync()
    {
        var items = await _data.GetRecipesAsync();
        foreach (var r in items) Available.Add(r);
    }

    [RelayCommand]
    private void ToggleSelect(RecipeData? r)
    {
        if (r is null) return;
        if (Selected.Contains(r)) Selected.Remove(r);
        else Selected.Add(r);
        r.IsFavorite = Selected.Contains(r);
    }

    [RelayCommand]
    private async Task Cancel() => await _navigator.NavigateBackAsync(this);

    [RelayCommand]
    private async Task CreateCookbookAction()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;
        var me = await _data.GetCurrentUserAsync();
        var cookbook = new CookbookData
        {
            Id = Guid.NewGuid(), UserId = me?.Id ?? Guid.Empty,
            Name = Name, Recipes = Selected.ToList(), PinsNumber = Selected.Count
        };
        await _data.SaveCookbookAsync(cookbook);
        await _navigator.NavigateBackAsync(this);
    }
}

public partial class UpdateCookbookViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;
    [ObservableProperty] private CookbookData? cookbook;
    [ObservableProperty] private string name = string.Empty;
    public ObservableCollection<RecipeData> Recipes { get; } = new();

    public UpdateCookbookViewModel(INavigator navigator, IChefsDataService data, CookbookData? cookbook = null)
    {
        _navigator = navigator; _data = data;
        Cookbook = cookbook;
        Name = cookbook?.Name ?? string.Empty;
        foreach (var r in cookbook?.Recipes ?? Enumerable.Empty<RecipeData>()) Recipes.Add(r);
    }

    [RelayCommand]
    private void RemoveRecipe(RecipeData? r) { if (r is not null) Recipes.Remove(r); }

    [RelayCommand]
    private async Task Cancel() => await _navigator.NavigateBackAsync(this);

    [RelayCommand]
    private async Task Apply()
    {
        if (Cookbook is null) return;
        Cookbook.Name = Name;
        Cookbook.Recipes = Recipes.ToList();
        Cookbook.PinsNumber = Recipes.Count;
        await _data.SaveCookbookAsync(Cookbook);
        await _navigator.NavigateBackAsync(this);
    }
}
