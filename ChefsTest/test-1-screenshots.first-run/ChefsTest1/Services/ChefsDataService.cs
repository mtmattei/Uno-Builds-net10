using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ChefsTest1.Models;
using Windows.ApplicationModel;
using Windows.Storage;

namespace ChefsTest1.Services;

public sealed class ChefsDataService : IChefsDataService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private List<RecipeData>? _recipes;
    private List<CookbookData>? _cookbooks;
    private List<UserData>? _users;
    private List<CategoryData>? _categories;
    private List<NotificationData>? _notifications;
    private HashSet<Guid>? _savedRecipes;
    private HashSet<Guid>? _savedCookbooks;

    public static string? LastError { get; private set; }

    private async Task<T> LoadAsync<T>(string fileName) where T : new()
    {
        try
        {
            var uri = new Uri($"ms-appx:///Assets/data/{fileName}");
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            using var stream = await file.OpenStreamForReadAsync();
            var result = await JsonSerializer.DeserializeAsync<T>(stream, JsonOpts);
            return result ?? new T();
        }
        catch (Exception ex)
        {
            LastError = $"{fileName}: {ex.GetType().Name}: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[ChefsDataService] {LastError}");
            return new T();
        }
    }

    public async Task<IReadOnlyList<RecipeData>> GetRecipesAsync()
    {
        if (_recipes == null)
        {
            _recipes = await LoadAsync<List<RecipeData>>("Recipes.json");
            await EnsureFavoritesAsync();
            foreach (var r in _recipes)
                r.IsFavorite = _savedRecipes!.Contains(r.Id);
        }
        return _recipes;
    }

    private async Task EnsureFavoritesAsync()
    {
        if (_savedRecipes == null)
        {
            var saved = await LoadAsync<List<Guid>>("SavedRecipes.json");
            _savedRecipes = new HashSet<Guid>(saved);
        }
    }

    public async Task<IReadOnlyList<RecipeData>> GetTrendingAsync()
    {
        var all = await GetRecipesAsync();
        return all.Take(10).ToList();
    }

    public async Task<IReadOnlyList<RecipeData>> GetPopularAsync()
    {
        var all = await GetRecipesAsync();
        return all.OrderByDescending(r => r.Reviews?.Count ?? 0).Take(10).ToList();
    }

    public async Task<IReadOnlyList<RecipeData>> GetRecentlyAddedAsync()
    {
        var all = await GetRecipesAsync();
        return all.OrderByDescending(r => r.Date).Take(12).ToList();
    }

    public async Task<IReadOnlyList<RecipeData>> GetFavoritedAsync()
    {
        var all = await GetRecipesAsync();
        await EnsureFavoritesAsync();
        return all.Where(r => _savedRecipes!.Contains(r.Id)).ToList();
    }

    public async Task<IReadOnlyList<CookbookData>> GetCookbooksAsync()
    {
        if (_cookbooks == null)
            _cookbooks = await LoadAsync<List<CookbookData>>("Cookbooks.json");
        return _cookbooks;
    }

    public async Task<IReadOnlyList<CookbookData>> GetSavedCookbooksAsync()
    {
        if (_savedCookbooks == null)
        {
            var ids = await LoadAsync<List<Guid>>("SavedCookbooks.json");
            _savedCookbooks = new HashSet<Guid>(ids);
        }
        var all = await GetCookbooksAsync();
        return all.Where(c => _savedCookbooks.Contains(c.Id)).ToList();
    }

    public async Task<IReadOnlyList<UserData>> GetUsersAsync()
    {
        if (_users == null)
            _users = await LoadAsync<List<UserData>>("Users.json");
        return _users;
    }

    public async Task<IReadOnlyList<UserData>> GetPopularCreatorsAsync()
    {
        var users = await GetUsersAsync();
        return users.OrderByDescending(u => u.Followers ?? 0).ToList();
    }

    public async Task<UserData> GetCurrentUserAsync()
    {
        var users = await GetUsersAsync();
        var current = users.FirstOrDefault(u => u.IsCurrent) ?? users.FirstOrDefault(u => u.FullName == "Niki Samantha") ?? users.First();
        current.IsCurrent = true;
        return current;
    }

    public async Task<IReadOnlyList<CategoryData>> GetCategoriesAsync()
    {
        if (_categories == null)
            _categories = await LoadAsync<List<CategoryData>>("categories.json");
        return _categories;
    }

    public async Task<IReadOnlyList<NotificationData>> GetNotificationsAsync()
    {
        if (_notifications == null)
            _notifications = await LoadAsync<List<NotificationData>>("Notifications.json");
        return _notifications;
    }

    public async Task<RecipeData?> GetRecipeAsync(Guid id)
    {
        var all = await GetRecipesAsync();
        return all.FirstOrDefault(r => r.Id == id);
    }

    public async Task<UserData?> GetUserAsync(Guid id)
    {
        var all = await GetUsersAsync();
        return all.FirstOrDefault(u => u.Id == id);
    }

    public async Task ToggleFavoriteAsync(Guid recipeId)
    {
        await EnsureFavoritesAsync();
        if (_savedRecipes!.Contains(recipeId))
            _savedRecipes.Remove(recipeId);
        else
            _savedRecipes.Add(recipeId);

        var recipes = await GetRecipesAsync();
        var r = recipes.FirstOrDefault(x => x.Id == recipeId);
        if (r != null)
            r.IsFavorite = _savedRecipes.Contains(recipeId);
    }
}
