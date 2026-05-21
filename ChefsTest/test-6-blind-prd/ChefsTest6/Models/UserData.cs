namespace ChefsTest6.Models;

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

    public string FollowerLabel => Followers.HasValue ? $"{Followers}" : "0";
    public string FollowingLabel => Following.HasValue ? $"{Following}" : "0";
    public string RecipeLabel => Recipes.HasValue ? $"{Recipes}" : "0";
}
