using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChefsTest4.Models;

namespace ChefsTest4.Services;

public interface IChefsDataService
{
    Task InitializeAsync();
    Task<IReadOnlyList<RecipeData>> GetAllRecipesAsync();
    Task<IReadOnlyList<RecipeData>> GetTrendingAsync();
    Task<IReadOnlyList<RecipeData>> GetPopularAsync();
    Task<IReadOnlyList<RecipeData>> GetRecentAsync();
    Task<IReadOnlyList<RecipeData>> GetFavoritedAsync();
    Task<IReadOnlyList<CategoryData>> GetCategoriesAsync();
    Task<IReadOnlyList<UserData>> GetUsersAsync();
    Task<IReadOnlyList<UserData>> GetPopularCreatorsAsync();
    Task<IReadOnlyList<CookbookData>> GetCookbooksAsync();
    Task<IReadOnlyList<CookbookData>> GetSavedCookbooksAsync();
    Task<IReadOnlyList<NotificationData>> GetNotificationsAsync();
    Task<RecipeData?> GetRecipeAsync(Guid id);
    Task<UserData?> GetUserAsync(Guid id);
    Task<UserData?> AuthenticateAsync(string email, string password);
    UserData? CurrentUser { get; }
    void SetCurrentUser(UserData user);
    Task ToggleFavoriteAsync(Guid recipeId);
    bool IsFavorite(Guid recipeId);
    Task SaveCookbookAsync(CookbookData cookbook);
    Task DeleteCookbookAsync(Guid cookbookId);
    bool NotificationsEnabled { get; set; }
    bool NightModeEnabled { get; set; }
    Guid? SelectedRecipeId { get; set; }
    Guid? SelectedCookbookId { get; set; }
    Guid? SelectedUserId { get; set; }
    event EventHandler? FavoritesChanged;
    event EventHandler? CookbooksChanged;
    event EventHandler? NightModeChanged;
}
