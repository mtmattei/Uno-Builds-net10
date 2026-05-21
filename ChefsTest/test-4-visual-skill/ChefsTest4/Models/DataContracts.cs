using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChefsTest4.Models;

// DESIGN.md §11 + API-CONTRACT.md DTO shapes. Records used for x:Bind compatibility (concrete types, immutable surfaces).

public partial record CategoryData
{
    public int? Id { get; init; }
    public string? UrlIcon { get; init; }
    public string? Name { get; init; }
    public string? Color { get; init; }
}

public partial record IngredientData
{
    public string? UrlIcon { get; init; }
    public string? Name { get; init; }
    public string? Quantity { get; init; }
}

public partial record StepData
{
    public string? UrlVideo { get; init; }
    public string? Name { get; init; }
    public int Number { get; init; }
    [JsonConverter(typeof(FlexibleTimeSpanConverter))]
    public TimeSpan CookTime { get; init; }
    public IReadOnlyList<string>? Cookware { get; init; }
    public IReadOnlyList<string>? Ingredients { get; init; }
    public string? Description { get; init; }
}

public partial record ReviewData
{
    public Guid Id { get; init; }
    public Guid RecipeId { get; init; }
    public string? UrlAuthorImage { get; init; }
    public Guid CreatedBy { get; init; }
    public string? PublisherName { get; init; }
    public DateTime Date { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<Guid>? Likes { get; init; }
    public IReadOnlyList<Guid>? Dislikes { get; init; }
    public bool? UserLike { get; init; }
}

public partial record NutritionData
{
    public double Protein { get; init; }
    public double ProteinBase { get; init; } = 110;
    public double Carbs { get; init; }
    public double CarbsBase { get; init; } = 300;
    public double Fat { get; init; }
    public double FatBase { get; init; } = 75;
}

public partial record UserData
{
    public Guid Id { get; init; }
    public string? UrlProfileImage { get; init; }
    public string? FullName { get; init; }
    public string? Description { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Password { get; init; }
    public long? Followers { get; init; }
    public long? Following { get; init; }
    public long? Recipes { get; init; }
    public bool IsCurrent { get; init; }
}

public partial record RecipeData
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public IReadOnlyList<StepData>? Steps { get; init; }
    public string? ImageUrl { get; init; }
    public string? Name { get; init; }
    public int Serves { get; init; }
    [JsonConverter(typeof(FlexibleTimeSpanConverter))]
    public TimeSpan CookTime { get; init; }
    public int Difficulty { get; init; }
    public IReadOnlyList<IngredientData>? Ingredients { get; init; }
    public string? Calories { get; init; }
    public IReadOnlyList<ReviewData>? Reviews { get; init; }
    public string? Details { get; init; }
    public CategoryData? Category { get; init; }
    public DateTime Date { get; init; }
    public bool IsFavorite { get; init; }
    public NutritionData? Nutrition { get; init; }
    public UserData? Creator { get; init; }
}

public partial record CookbookData
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string? Name { get; init; }
    public IReadOnlyList<RecipeData>? Recipes { get; init; }
    public int? PinsNumber { get; init; }
}

public partial record NotificationData
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public bool IsRead { get; init; }
    public DateTime Date { get; init; }
}

// CookTime appears either as {"ticks": N} (Recipes.json) or "00:03:00" (Cookbooks.json) — accept both.
public sealed class FlexibleTimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (TimeSpan.TryParse(s, out var ts)) return ts;
            return TimeSpan.Zero;
        }
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            long ticks = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject) break;
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var name = reader.GetString();
                    reader.Read();
                    if (string.Equals(name, "ticks", StringComparison.OrdinalIgnoreCase) && reader.TokenType == JsonTokenType.Number)
                    {
                        ticks = reader.GetInt64();
                    }
                }
            }
            return TimeSpan.FromTicks(ticks);
        }
        if (reader.TokenType == JsonTokenType.Number)
        {
            return TimeSpan.FromTicks(reader.GetInt64());
        }
        return TimeSpan.Zero;
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
