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

public partial class UpdateCookbookViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsService _service;
    private readonly CookbookData _cookbook;

    [ObservableProperty]
    private string _name;

    public ObservableCollection<RecipeSelection> Recipes { get; } = new();

    public UpdateCookbookViewModel(INavigator navigator, IChefsService service, CookbookData cookbook)
    {
        _navigator = navigator;
        _service = service;
        _cookbook = cookbook ?? new CookbookData { Name = "Cookbook" };
        _name = _cookbook.Name ?? string.Empty;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var all = await _service.GetAllRecipesAsync();
        var existing = (_cookbook.Recipes ?? new List<RecipeData>()).Select(r => r.Id).ToHashSet();
        Recipes.Clear();
        foreach (var r in all.Take(20))
        {
            Recipes.Add(new RecipeSelection(r, existing.Contains(r.Id)));
        }
    }

    [RelayCommand]
    private async void Cancel()
    {
        await _navigator.NavigateBackAsync(this);
    }

    [RelayCommand]
    private async void Apply()
    {
        var selected = Recipes.Where(r => r.IsSelected).Select(r => r.Recipe).ToList();
        _cookbook.Name = string.IsNullOrWhiteSpace(Name) ? _cookbook.Name : Name;
        _cookbook.Recipes = selected;
        _cookbook.PinsNumber = selected.Count;
        await _service.UpdateCookbookAsync(_cookbook);
        await _navigator.NavigateBackAsync(this);
    }
}
