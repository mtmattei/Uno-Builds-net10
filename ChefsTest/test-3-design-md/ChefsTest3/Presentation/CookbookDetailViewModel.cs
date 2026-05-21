using System.Collections.ObjectModel;
using ChefsTest3.Models;
using ChefsTest3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest3.Presentation;

public partial class CookbookDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsService _service;

    [ObservableProperty]
    private CookbookData _cookbook;

    public ObservableCollection<RecipeData> Recipes { get; } = new();

    public string CountLabel => $"{Recipes.Count} recipes";

    public CookbookDetailViewModel(INavigator navigator, IChefsService service, CookbookData cookbook)
    {
        _navigator = navigator;
        _service = service;
        _cookbook = cookbook ?? new CookbookData { Name = "Cookbook" };
        if (_cookbook.Recipes is not null)
        {
            foreach (var r in _cookbook.Recipes) Recipes.Add(r);
        }
        OnPropertyChanged(nameof(CountLabel));
    }

    [RelayCommand]
    private async void OpenRecipe(RecipeData recipe)
    {
        if (recipe is null) return;
        await _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: recipe);
    }

    [RelayCommand]
    private async void GoBack()
    {
        await _navigator.NavigateBackAsync(this);
    }

    [RelayCommand]
    private async void Edit()
    {
        await _navigator.NavigateViewModelAsync<UpdateCookbookViewModel>(this, data: Cookbook);
    }
}
