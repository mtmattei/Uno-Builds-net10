using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChefsTest1.Models;

public class CategoryData
{
    public int? Id { get; set; }
    public string? UrlIcon { get; set; }
    public string? Name { get; set; }
    public string? Color { get; set; }
}

public class IngredientData
{
    public string? UrlIcon { get; set; }
    public string? Name { get; set; }
    public string? Quantity { get; set; }
}

public class StepData
{
    public string? UrlVideo { get; set; }
    public string? Name { get; set; }
    public int Number { get; set; }

    [JsonConverter(typeof(FlexibleTimeSpanConverter))]
    public TimeSpan CookTime { get; set; }

    public List<string>? Cookware { get; set; }
    public List<string>? Ingredients { get; set; }
    public string? Description { get; set; }
}

public class NutritionData
{
    public double Protein { get; set; }
    public double ProteinBase { get; set; }
    public double Carbs { get; set; }
    public double CarbsBase { get; set; }
    public double Fat { get; set; }
    public double FatBase { get; set; }
}

public class ReviewData
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }
    public string? UrlAuthorImage { get; set; }
    public Guid CreatedBy { get; set; }
    public string? PublisherName { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public List<Guid>? Likes { get; set; }
    public List<Guid>? Dislikes { get; set; }
    public bool? UserLike { get; set; }
}

public class UserData
{
    public Guid Id { get; set; }
    public string? UrlProfileImage { get; set; }
    public string? FullName { get; set; }
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public long? Followers { get; set; }
    public long? Following { get; set; }
    public long? Recipes { get; set; }
    public bool IsCurrent { get; set; }
}

public class RecipeData
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<StepData>? Steps { get; set; }
    public string? ImageUrl { get; set; }
    public string? Name { get; set; }
    public int Serves { get; set; }

    [JsonConverter(typeof(FlexibleTimeSpanConverter))]
    public TimeSpan CookTime { get; set; }

    public int Difficulty { get; set; }
    public List<IngredientData>? Ingredients { get; set; }
    public string? Calories { get; set; }
    public List<ReviewData>? Reviews { get; set; }
    public string? Details { get; set; }
    public CategoryData? Category { get; set; }
    public DateTime Date { get; set; }
    public bool IsFavorite { get; set; }
    public NutritionData? Nutrition { get; set; }
    public UserData? Creator { get; set; }
    public bool Save { get; set; }

    public string CookTimeDisplay => CookTime.TotalMinutes >= 1
        ? $"{(int)CookTime.TotalMinutes} mins"
        : "0 mins";

    public string DifficultyLabel => Difficulty switch
    {
        0 => "Easy",
        1 => "Easy",
        2 => "Medium",
        3 => "Hard",
        _ => "Easy"
    };

    public string CookTimeAndCalories => $"{(int)CookTime.TotalMinutes} mins • {Calories}";
}

public class CookbookData
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public int? PinsNumber { get; set; }
    public List<RecipeData>? Recipes { get; set; }

    public int RecipeCount => Recipes?.Count ?? 0;
    public string RecipeCountDisplay => $"{RecipeCount} recipes";

    public string? Cover0 => Recipes != null && Recipes.Count > 0 ? Recipes[0].ImageUrl : null;
    public string? Cover1 => Recipes != null && Recipes.Count > 1 ? Recipes[1].ImageUrl : null;
    public string? Cover2 => Recipes != null && Recipes.Count > 2 ? Recipes[2].ImageUrl : null;
    public string? Cover3 => Recipes != null && Recipes.Count > 3 ? Recipes[3].ImageUrl : null;
}

public class NotificationData
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsRead { get; set; }
    public DateTime Date { get; set; }
}

public class LoginRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

internal sealed class FlexibleTimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrEmpty(s))
                return TimeSpan.Zero;
            return TimeSpan.Parse(s);
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            long ticks = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var name = reader.GetString();
                    reader.Read();
                    if (string.Equals(name, "ticks", StringComparison.OrdinalIgnoreCase))
                        ticks = reader.GetInt64();
                }
            }
            return new TimeSpan(ticks);
        }

        return TimeSpan.Zero;
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
