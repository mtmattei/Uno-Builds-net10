using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChefsTest5.Services;

public interface IChefsDataService
{
    Task<IReadOnlyList<RecipeData>> GetRecipesAsync();
    Task<IReadOnlyList<RecipeData>> GetTrendingAsync();
    Task<IReadOnlyList<RecipeData>> GetPopularAsync();
    Task<IReadOnlyList<RecipeData>> GetRecentAsync();
    Task<IReadOnlyList<RecipeData>> GetFavoritedAsync();
    Task<IReadOnlyList<CategoryData>> GetCategoriesAsync();
    Task<IReadOnlyList<CookbookData>> GetCookbooksAsync();
    Task<IReadOnlyList<CookbookData>> GetSavedCookbooksAsync();
    Task<IReadOnlyList<UserData>> GetUsersAsync();
    Task<IReadOnlyList<UserData>> GetPopularCreatorsAsync();
    Task<UserData?> GetCurrentUserAsync();
    Task<IReadOnlyList<NotificationData>> GetNotificationsAsync();
    Task<RecipeData?> GetRecipeAsync(Guid id);
    Task ToggleFavoriteAsync(Guid recipeId);
    Task SetNightModeAsync(bool enabled);
    bool IsNightMode { get; }
    event EventHandler<bool>? NightModeChanged;
}
