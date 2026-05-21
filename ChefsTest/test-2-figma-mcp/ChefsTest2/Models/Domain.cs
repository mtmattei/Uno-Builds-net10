using System.Text.Json.Serialization;

namespace ChefsTest2.Models;

public sealed class Category
{
    public int Id { get; set; }
    public string UrlIcon { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#7A67F8";
}

public sealed class User
{
    public string Id { get; set; } = string.Empty;
    public string UrlProfileImage { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public int Followers { get; set; }
    public int Following { get; set; }
    public int Recipes { get; set; }
}

public sealed class Ingredient
{
    public string UrlIcon { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
}

public sealed class Step
{
    public string Name { get; set; } = string.Empty;
    [JsonConverter(typeof(FlexibleTimeSpanConverter))]
    public TimeSpan CookTime { get; set; }
    public List<string> Cookware { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public List<string> Ingredients { get; set; } = new();
    public int Number { get; set; }
    public string UrlVideo { get; set; } = string.Empty;
}

public sealed class Review
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UrlProfileImage { get; set; } = string.Empty;
    public int Stars { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
}

public sealed class Recipe
{
    public string Name { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public List<Step> Steps { get; set; } = new();
    public string ImageUrl { get; set; } = string.Empty;
    public int Serves { get; set; }
    [JsonConverter(typeof(FlexibleTimeSpanConverter))]
    public TimeSpan CookTime { get; set; }
    public int Difficulty { get; set; }
    public List<Ingredient> Ingredients { get; set; } = new();
    public string Calories { get; set; } = string.Empty;
    public List<Review> Reviews { get; set; } = new();
    public string Details { get; set; } = string.Empty;
    public User Creator { get; set; } = new();
    public Category Category { get; set; } = new();
    public DateTimeOffset Date { get; set; }
    public bool Save { get; set; }
}

public sealed class Cookbook
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public int PinsNumber { get; set; }
    public List<Recipe> Recipes { get; set; } = new();
}

public sealed class Notification
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTimeOffset Date { get; set; }
}

internal sealed class FlexibleTimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        if (reader.TokenType == System.Text.Json.JsonTokenType.String)
        {
            return TimeSpan.Parse(reader.GetString()!);
        }
        if (reader.TokenType == System.Text.Json.JsonTokenType.StartObject)
        {
            long ticks = 0;
            while (reader.Read() && reader.TokenType != System.Text.Json.JsonTokenType.EndObject)
            {
                if (reader.TokenType == System.Text.Json.JsonTokenType.PropertyName
                    && string.Equals(reader.GetString(), "ticks", StringComparison.OrdinalIgnoreCase))
                {
                    reader.Read();
                    ticks = reader.GetInt64();
                }
            }
            return TimeSpan.FromTicks(ticks);
        }
        return TimeSpan.Zero;
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, TimeSpan value, System.Text.Json.JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
