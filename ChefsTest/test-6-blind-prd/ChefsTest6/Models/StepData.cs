using System.Text.Json.Serialization;

namespace ChefsTest6.Models;

public class StepData
{
    public string? Name { get; set; }
    public int Number { get; set; }
    public string? Description { get; set; }
    public string? UrlVideo { get; set; }

    [JsonConverter(typeof(FlexibleTimeSpanConverter))]
    public TimeSpan CookTime { get; set; }

    public List<string>? Cookware { get; set; }
    public List<string>? Ingredients { get; set; }

    public string Title => string.IsNullOrEmpty(Name) ? $"Step {Number}" : $"{Number}. {Name}";
}
