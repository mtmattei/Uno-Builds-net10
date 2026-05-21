using System.Text.Json.Serialization;

namespace ChefsTest6.Models;

public partial class RecipeData : ObservableObject
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? ImageUrl { get; set; }
    public int Serves { get; set; }

    [JsonConverter(typeof(FlexibleTimeSpanConverter))]
    public TimeSpan CookTime { get; set; }

    public int Difficulty { get; set; }
    public string? Calories { get; set; }
    public string? Details { get; set; }
    public DateTime Date { get; set; }

    public List<StepData>? Steps { get; set; }
    public List<IngredientData>? Ingredients { get; set; }
    public List<ReviewData>? Reviews { get; set; }
    public NutritionData? Nutrition { get; set; }
    public CategoryData? Category { get; set; }
    public UserData? Creator { get; set; }

    [JsonPropertyName("Save")]
    public bool? SaveFlag { get; set; }

    [ObservableProperty]
    [property: JsonIgnore]
    private bool isFavorite;

    public string DifficultyLabel => Difficulty switch
    {
        1 => "Beginner",
        2 => "Intermediate",
        3 => "Advanced",
        _ => "Beginner"
    };

    public string CookTimeLabel
    {
        get
        {
            var minutes = (int)Math.Round(CookTime.TotalMinutes);
            return minutes <= 0 ? "—" : $"{minutes} min";
        }
    }

    public int CookTimeMinutes => (int)Math.Round(CookTime.TotalMinutes);
}
