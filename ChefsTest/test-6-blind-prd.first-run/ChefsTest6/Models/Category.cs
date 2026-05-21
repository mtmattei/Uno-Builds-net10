namespace ChefsTest6.Models;

public partial record Category
{
    public int? Id { get; init; }
    public string? UrlIcon { get; init; }
    public string? Name { get; init; }
    public string? Color { get; init; }
}
