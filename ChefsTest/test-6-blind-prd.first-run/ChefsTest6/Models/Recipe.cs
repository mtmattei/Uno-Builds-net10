namespace ChefsTest6.Models;

public partial class Recipe : ObservableObject
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string? Name { get; init; }
    public string? ImageUrl { get; init; }
    public int Serves { get; init; }
    public TimeSpan CookTime { get; init; }
    public int Difficulty { get; init; }
    public string? Calories { get; init; }
    public string? Details { get; init; }
    public DateTime Date { get; init; }
    public Category? Category { get; init; }
    public User? Creator { get; init; }
    public Nutrition? Nutrition { get; init; }
    public IReadOnlyList<Ingredient> Ingredients { get; init; } = Array.Empty<Ingredient>();
    public IReadOnlyList<CookingStep> Steps { get; init; } = Array.Empty<CookingStep>();
    public IReadOnlyList<Review> Reviews { get; init; } = Array.Empty<Review>();

    [ObservableProperty]
    private bool isFavorite;

    public string CookTimeText
    {
        get
        {
            var minutes = (int)Math.Round(CookTime.TotalMinutes);
            return minutes <= 0 ? "—" : $"{minutes} min";
        }
    }

    public string DifficultyText => Difficulty switch
    {
        0 => "Beginner",
        1 => "Intermediate",
        _ => "Advanced",
    };
}
