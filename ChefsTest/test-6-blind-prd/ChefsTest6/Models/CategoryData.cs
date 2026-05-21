namespace ChefsTest6.Models;

public class CategoryData
{
    public int? Id { get; set; }
    public string? UrlIcon { get; set; }
    public string? Name { get; set; }
    public string? Color { get; set; }

    public int RecipeCount { get; set; }

    public string CountLabel => RecipeCount > 0 ? $"{RecipeCount} recipes" : "—";
}
