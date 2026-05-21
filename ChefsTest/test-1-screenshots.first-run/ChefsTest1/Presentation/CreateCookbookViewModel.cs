using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChefsTest1.Models;
using ChefsTest1.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class CreateCookbookViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private string? cookbookName;
    [ObservableProperty] private ObservableCollection<RecipeData> recipes = new();

    public CreateCookbookViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        _ = Load();
    }

    private async Task Load()
    {
        var all = await _data.GetRecipesAsync();
        Recipes = new ObservableCollection<RecipeData>(all);
    }

    [RelayCommand]
    private Task Cancel() => _navigator.NavigateBackAsync(this);

    [RelayCommand]
    private Task Create() => _navigator.NavigateBackAsync(this);

    [RelayCommand]
    private async Task ToggleFav(RecipeData? r)
    {
        if (r == null) return;
        await _data.ToggleFavoriteAsync(r.Id);
        var i = Recipes.IndexOf(r);
        if (i >= 0) { Recipes.RemoveAt(i); Recipes.Insert(i, r); }
    }
}
