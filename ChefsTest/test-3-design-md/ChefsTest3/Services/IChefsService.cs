using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChefsTest3.Models;

namespace ChefsTest3.Services;

public interface IChefsService
{
    Task<IReadOnlyList<RecipeData>> GetAllRecipesAsync();
    Task<IReadOnlyList<RecipeData>> GetTrendingAsync();
    Task<IReadOnlyList<RecipeData>> GetPopularAsync();
    Task<IReadOnlyList<RecipeData>> GetRecentlyAddedAsync();
    Task<IReadOnlyList<RecipeData>> GetFavoriteRecipesAsync();
    Task<RecipeData?> GetRecipeByIdAsync(Guid id);
    Task ToggleFavoriteRecipeAsync(Guid recipeId);
    Task<IReadOnlyList<CategoryData>> GetCategoriesAsync();
    Task<IReadOnlyList<CookbookData>> GetCookbooksAsync();
    Task<IReadOnlyList<CookbookData>> GetSavedCookbooksAsync();
    Task SaveCookbookAsync(CookbookData cookbook);
    Task UpdateCookbookAsync(CookbookData cookbook);
    Task<IReadOnlyList<UserData>> GetUsersAsync();
    Task<IReadOnlyList<UserData>> GetPopularCreatorsAsync();
    Task<UserData> GetCurrentUserAsync();
    Task UpdateCurrentUserAsync(UserData user);
    Task<IReadOnlyList<NotificationData>> GetNotificationsAsync();
    Task<IReadOnlyList<RecipeData>> SearchRecipesAsync(string query);
}
