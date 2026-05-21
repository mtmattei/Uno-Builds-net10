using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ChefsTest1.Models;
using ChefsTest1.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class ProfileViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private UserData? user;
    [ObservableProperty] private ObservableCollection<RecipeData> recipes = new();
    [ObservableProperty] private bool hasRecipes;

    public ProfileViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        _ = Load();
    }

    private async Task Load()
    {
        User = await _data.GetCurrentUserAsync();
        var all = await _data.GetRecipesAsync();
        var mine = all.Where(r => r.UserId == User.Id).ToList();
        Recipes = new ObservableCollection<RecipeData>(mine);
        HasRecipes = mine.Count > 0;
        if (User != null)
        {
            User.Recipes = mine.Count;
        }
    }

    [RelayCommand] private Task Back() => _navigator.NavigateBackAsync(this);
    [RelayCommand] private Task OpenSettings() => _navigator.NavigateViewModelAsync<SettingsViewModel>(this);
    [RelayCommand] private Task OpenRecipe(RecipeData? r) =>
        r == null ? Task.CompletedTask : _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: r);
    [RelayCommand] private Task CreateRecipe() => Task.CompletedTask;
}
