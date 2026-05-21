namespace ChefsTest6.Models;

public record Ingredient
{
    public string? UrlIcon { get; init; }
    public string? Name { get; init; }
    public string? Quantity { get; init; }
}

public record CookingStep
{
    public string? UrlVideo { get; init; }
    public string? Name { get; init; }
    public int Number { get; init; }
    public TimeSpan CookTime { get; init; }
    public IReadOnlyList<string>? Cookware { get; init; }
    public IReadOnlyList<string>? Ingredients { get; init; }
    public string? Description { get; init; }
}

public record Nutrition
{
    public double Protein { get; init; }
    public double ProteinBase { get; init; }
    public double Carbs { get; init; }
    public double CarbsBase { get; init; }
    public double Fat { get; init; }
    public double FatBase { get; init; }
}
