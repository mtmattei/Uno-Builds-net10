using ChefsTest2.Models;

namespace ChefsTest2.Services;

public interface IDataService
{
    Task<IReadOnlyList<Recipe>> GetRecipesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Cookbook>> GetCookbooksAsync(CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetUsersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> GetNotificationsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetSavedRecipeIdsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetSavedCookbookIdsAsync(CancellationToken ct = default);

    User CurrentUser { get; }
    HashSet<string> FavoriteRecipeIds { get; }
    HashSet<string> SavedCookbookIds { get; }
    void ToggleFavoriteRecipe(string id);
    void ToggleSavedCookbook(string id);
}
