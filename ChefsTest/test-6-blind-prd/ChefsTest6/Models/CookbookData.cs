using System.Collections.ObjectModel;

namespace ChefsTest6.Models;

public partial class CookbookData : ObservableObject
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    [ObservableProperty]
    private string? name;

    public ObservableCollection<RecipeData> Recipes { get; set; } = new();

    public int RecipeCount => Recipes.Count;

    public string CountLabel => RecipeCount switch
    {
        0 => "No recipes",
        1 => "1 recipe",
        _ => $"{RecipeCount} recipes"
    };

    public string? Preview1 => Recipes.Count > 0 ? Recipes[0].ImageUrl : null;
    public string? Preview2 => Recipes.Count > 1 ? Recipes[1].ImageUrl : null;
    public string? Preview3 => Recipes.Count > 2 ? Recipes[2].ImageUrl : null;
    public string? Preview4 => Recipes.Count > 3 ? Recipes[3].ImageUrl : null;
}
