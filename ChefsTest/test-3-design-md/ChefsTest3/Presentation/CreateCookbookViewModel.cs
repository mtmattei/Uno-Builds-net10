using System;
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

public partial class CreateCookbookViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsService _service;

    [ObservableProperty]
    private string _name = string.Empty;

    public ObservableCollection<RecipeSelection> Recipes { get; } = new();

    public CreateCookbookViewModel(INavigator navigator, IChefsService service)
    {
        _navigator = navigator;
        _service = service;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var all = await _service.GetAllRecipesAsync();
        Recipes.Clear();
        foreach (var r in all.Take(20)) Recipes.Add(new RecipeSelection(r, false));
    }

    [RelayCommand]
    private async void Cancel()
    {
        await _navigator.NavigateBackAsync(this);
    }

    [RelayCommand]
    private async void Create()
    {
        var selected = Recipes.Where(r => r.IsSelected).Select(r => r.Recipe).ToList();
        var book = new CookbookData
        {
            Id = Guid.NewGuid(),
            Name = string.IsNullOrWhiteSpace(Name) ? "New Cookbook" : Name,
            PinsNumber = selected.Count,
            Recipes = selected,
        };
        await _service.SaveCookbookAsync(book);
        await _navigator.NavigateBackAsync(this);
    }
}

public partial class RecipeSelection : ObservableObject
{
    public RecipeData Recipe { get; }

    [ObservableProperty]
    private bool _isSelected;

    public RecipeSelection(RecipeData recipe, bool isSelected)
    {
        Recipe = recipe;
        _isSelected = isSelected;
    }
}
