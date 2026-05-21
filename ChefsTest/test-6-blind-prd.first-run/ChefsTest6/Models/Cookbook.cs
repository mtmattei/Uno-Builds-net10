using System.Collections.ObjectModel;

namespace ChefsTest6.Models;

public partial class Cookbook : ObservableObject
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }

    [ObservableProperty]
    private string? name;

    public ObservableCollection<Recipe> Recipes { get; } = new();

    public string? Cover1 => Recipes.Count > 0 ? Recipes[0].ImageUrl : null;
    public string? Cover2 => Recipes.Count > 1 ? Recipes[1].ImageUrl : null;
    public string? Cover3 => Recipes.Count > 2 ? Recipes[2].ImageUrl : null;
    public string? Cover4 => Recipes.Count > 3 ? Recipes[3].ImageUrl : null;

    public int RecipeCount => Recipes.Count;

    public void RaiseCovers()
    {
        OnPropertyChanged(nameof(Cover1));
        OnPropertyChanged(nameof(Cover2));
        OnPropertyChanged(nameof(Cover3));
        OnPropertyChanged(nameof(Cover4));
        OnPropertyChanged(nameof(RecipeCount));
    }
}
