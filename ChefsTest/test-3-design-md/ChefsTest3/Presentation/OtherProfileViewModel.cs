using System.Collections.ObjectModel;
using System.Linq;
using ChefsTest3.Models;
using ChefsTest3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest3.Presentation;

public partial class OtherProfileViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsService _service;

    [ObservableProperty]
    private UserData _user;

    public ObservableCollection<RecipeData> Recipes { get; } = new();

    [ObservableProperty]
    private bool _hasRecipes = true;

    public OtherProfileViewModel(INavigator navigator, IChefsService service, UserData user)
    {
        _navigator = navigator;
        _service = service;
        _user = user ?? new UserData { FullName = "User" };
        _ = LoadAsync();
    }

    private async System.Threading.Tasks.Task LoadAsync()
    {
        var all = await _service.GetAllRecipesAsync();
        Recipes.Clear();
        foreach (var r in all.Where(x => x.UserId == User.Id).Take(20)) Recipes.Add(r);
        if (Recipes.Count == 0)
        {
            foreach (var r in all.Take(4)) Recipes.Add(r);
        }
        HasRecipes = Recipes.Count > 0;
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
}
