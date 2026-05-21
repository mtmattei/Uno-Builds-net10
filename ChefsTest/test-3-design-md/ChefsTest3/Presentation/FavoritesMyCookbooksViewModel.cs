using System.Collections.ObjectModel;
using ChefsTest3.Models;
using ChefsTest3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest3.Presentation;

public partial class FavoritesMyCookbooksViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsService _service;

    public ObservableCollection<CookbookData> Cookbooks { get; } = new();

    [ObservableProperty]
    private bool _hasItems = true;

    public FavoritesMyCookbooksViewModel(INavigator navigator, IChefsService service)
    {
        _navigator = navigator;
        _service = service;
        _ = LoadAsync();
    }

    private async System.Threading.Tasks.Task LoadAsync()
    {
        var books = await _service.GetCookbooksAsync();
        Cookbooks.Clear();
        foreach (var c in books) Cookbooks.Add(c);
        HasItems = Cookbooks.Count > 0;
    }

    [RelayCommand]
    private async void OpenCookbook(CookbookData cookbook)
    {
        if (cookbook is null) return;
        await _navigator.NavigateViewModelAsync<CookbookDetailViewModel>(this, data: cookbook);
    }

    [RelayCommand]
    private async void GoToAllRecipes()
    {
        await _navigator.NavigateRouteAsync(this, "FavoritesAll", qualifier: Qualifiers.Root);
    }

    [RelayCommand]
    private async void CreateCookbook()
    {
        await _navigator.NavigateRouteAsync(this, "CreateCookbook");
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
