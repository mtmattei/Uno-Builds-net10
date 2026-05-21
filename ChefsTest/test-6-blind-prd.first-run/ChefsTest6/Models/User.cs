namespace ChefsTest6.Models;

public partial record User
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

public record LoginRequest(string? Email, string? Password);
