using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChefsTest1.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class CookbookDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty] private CookbookData? cookbook;
    [ObservableProperty] private ObservableCollection<RecipeData> recipes = new();
    [ObservableProperty] private string resultCount = "0 results";

    public CookbookDetailViewModel(INavigator navigator, CookbookData? cookbook = null)
    {
        _navigator = navigator;
        if (cookbook != null) Bind(cookbook);
    }

    public void Bind(CookbookData? c)
    {
        Cookbook = c;
        if (c?.Recipes != null)
        {
            Recipes = new ObservableCollection<RecipeData>(c.Recipes);
            ResultCount = $"{c.Recipes.Count} results";
        }
    }

    [RelayCommand]
    private Task Back() => _navigator.NavigateBackAsync(this);

    [RelayCommand]
    private Task Edit() => _navigator.NavigateViewModelAsync<UpdateCookbookViewModel>(this, data: Cookbook);

    [RelayCommand]
    private Task OpenRecipe(RecipeData? r) =>
        r == null ? Task.CompletedTask : _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: r);
}
