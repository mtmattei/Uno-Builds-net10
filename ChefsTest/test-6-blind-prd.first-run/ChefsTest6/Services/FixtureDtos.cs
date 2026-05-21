using System.Text.Json.Serialization;

namespace ChefsTest6.Services;

internal sealed class FixtureRecipe
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? ImageUrl { get; set; }
    public int Serves { get; set; }
    [JsonConverter(typeof(TimeSpanFlexConverter))]
    public TimeSpan CookTime { get; set; }
    public int Difficulty { get; set; }
    public string? Calories { get; set; }
    public string? Details { get; set; }
    public DateTime Date { get; set; }
    public bool Save { get; set; }
    public Models.Category? Category { get; set; }
    public FixtureUser? Creator { get; set; }
    public Models.Nutrition? Nutrition { get; set; }
    public List<FixtureIngredient>? Ingredients { get; set; }
    public List<FixtureStep>? Steps { get; set; }
    public List<FixtureReview>? Reviews { get; set; }
}

internal sealed class FixtureUser
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
}

internal sealed class FixtureIngredient
{
    public string? UrlIcon { get; set; }
    public string? Name { get; set; }
    public string? Quantity { get; set; }
}

internal sealed class FixtureStep
{
    public string? UrlVideo { get; set; }
    public string? Name { get; set; }
    public int Number { get; set; }
    [JsonConverter(typeof(TimeSpanFlexConverter))]
    public TimeSpan CookTime { get; set; }
    public List<string>? Cookware { get; set; }
    public List<string>? Ingredients { get; set; }
    public string? Description { get; set; }
}

internal sealed class FixtureReview
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
}

internal sealed class FixtureCookbook
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public List<FixtureRecipe>? Recipes { get; set; }
}

internal sealed class FixtureNotification
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsRead { get; set; }
    public DateTime Date { get; set; }
}
