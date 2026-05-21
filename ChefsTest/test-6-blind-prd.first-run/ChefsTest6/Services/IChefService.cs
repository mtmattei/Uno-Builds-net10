namespace ChefsTest6.Services;

public interface IChefService
{
    Task InitializeAsync();

    // Recipes
    IReadOnlyList<Recipe> AllRecipes { get; }
    IReadOnlyList<Category> Categories { get; }
    IReadOnlyList<Recipe> Trending { get; }
    IReadOnlyList<Recipe> Popular { get; }
    IReadOnlyList<Recipe> RecentlyAdded { get; }
    IReadOnlyList<Recipe> Favorites { get; }
    void ToggleFavorite(Recipe recipe);
    Recipe? GetRecipe(Guid id);
    IReadOnlyList<Review> GetReviews(Guid recipeId);
    void ToggleLike(Review review);
    void ToggleDislike(Review review);
    IReadOnlyList<Recipe> Search(string? query, SearchFilters filters);

    // Cookbooks
    IReadOnlyList<Cookbook> AllCookbooks { get; }
    IReadOnlyList<Cookbook> SavedCookbooks { get; }
    Cookbook CreateCookbook(string name, IEnumerable<Recipe> recipes);
    void UpdateCookbook(Cookbook cookbook, string name, IEnumerable<Recipe> recipes);

    // Users
    IReadOnlyList<User> AllUsers { get; }
    IReadOnlyList<User> PopularContributors { get; }
    User? CurrentUser { get; }
    Task<User?> AuthenticateAsync(LoginRequest request);
    User? GetUser(Guid id);
    void UpdateCurrentUser(User updated);
    IReadOnlyList<Recipe> GetRecipesByAuthor(Guid userId);

    // Notifications
    IReadOnlyList<Notification> Notifications { get; }
}
