using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ChefsTest3.Models;
using Windows.ApplicationModel;
using Windows.Storage;

namespace ChefsTest3.Services;

public sealed class ChefsService : IChefsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private readonly SemaphoreSlim _gate = new(1, 1);

    private List<RecipeData>? _recipes;
    private List<CategoryData>? _categories;
    private List<CookbookData>? _cookbooks;
    private List<UserData>? _users;
    private List<NotificationData>? _notifications;
    private HashSet<Guid> _savedRecipeIds = new();
    private HashSet<Guid> _savedCookbookIds = new();
    private UserData? _currentUser;

    public async Task<IReadOnlyList<RecipeData>> GetAllRecipesAsync()
    {
        await EnsureLoadedAsync();
        return _recipes!;
    }

    public async Task<IReadOnlyList<RecipeData>> GetTrendingAsync()
    {
        await EnsureLoadedAsync();
        // Pick up to 8 with the most reviews to fake "trending"
        return _recipes!
            .OrderByDescending(r => (r.Reviews?.Count ?? 0))
            .Take(8)
            .ToList();
    }

    public async Task<IReadOnlyList<RecipeData>> GetPopularAsync()
    {
        await EnsureLoadedAsync();
        return _recipes!
            .OrderBy(r => r.Difficulty)
            .ThenBy(r => r.CookTime)
            .Take(8)
            .ToList();
    }

    public async Task<IReadOnlyList<RecipeData>> GetRecentlyAddedAsync()
    {
        await EnsureLoadedAsync();
        return _recipes!.OrderByDescending(r => r.Date).Take(8).ToList();
    }

    public async Task<IReadOnlyList<RecipeData>> GetFavoriteRecipesAsync()
    {
        await EnsureLoadedAsync();
        return _recipes!.Where(r => _savedRecipeIds.Contains(r.Id) || r.IsFavorite).ToList();
    }

    public async Task<RecipeData?> GetRecipeByIdAsync(Guid id)
    {
        await EnsureLoadedAsync();
        return _recipes!.FirstOrDefault(r => r.Id == id);
    }

    public async Task ToggleFavoriteRecipeAsync(Guid recipeId)
    {
        await EnsureLoadedAsync();
        if (!_savedRecipeIds.Add(recipeId))
        {
            _savedRecipeIds.Remove(recipeId);
        }
        var recipe = _recipes!.FirstOrDefault(r => r.Id == recipeId);
        if (recipe is not null)
        {
            recipe.IsFavorite = _savedRecipeIds.Contains(recipeId);
        }
    }

    public async Task<IReadOnlyList<CategoryData>> GetCategoriesAsync()
    {
        await EnsureLoadedAsync();
        return _categories!;
    }

    public async Task<IReadOnlyList<CookbookData>> GetCookbooksAsync()
    {
        await EnsureLoadedAsync();
        return _cookbooks!;
    }

    public async Task<IReadOnlyList<CookbookData>> GetSavedCookbooksAsync()
    {
        await EnsureLoadedAsync();
        return _cookbooks!.Where(c => _savedCookbookIds.Contains(c.Id)).ToList();
    }

    public async Task SaveCookbookAsync(CookbookData cookbook)
    {
        await EnsureLoadedAsync();
        if (cookbook.Id == Guid.Empty) cookbook.Id = Guid.NewGuid();
        _cookbooks!.Insert(0, cookbook);
        _savedCookbookIds.Add(cookbook.Id);
    }

    public async Task UpdateCookbookAsync(CookbookData cookbook)
    {
        await EnsureLoadedAsync();
        var idx = _cookbooks!.FindIndex(c => c.Id == cookbook.Id);
        if (idx >= 0) _cookbooks[idx] = cookbook;
    }

    public async Task<IReadOnlyList<UserData>> GetUsersAsync()
    {
        await EnsureLoadedAsync();
        return _users!;
    }

    public async Task<IReadOnlyList<UserData>> GetPopularCreatorsAsync()
    {
        await EnsureLoadedAsync();
        return _users!
            .Where(u => (u.Recipes ?? 0) > 0)
            .OrderByDescending(u => u.Followers ?? 0)
            .Take(8)
            .ToList();
    }

    public async Task<UserData> GetCurrentUserAsync()
    {
        await EnsureLoadedAsync();
        return _currentUser!;
    }

    public async Task UpdateCurrentUserAsync(UserData user)
    {
        await EnsureLoadedAsync();
        _currentUser = user;
    }

    public async Task<IReadOnlyList<NotificationData>> GetNotificationsAsync()
    {
        await EnsureLoadedAsync();
        return _notifications!;
    }

    public async Task<IReadOnlyList<RecipeData>> SearchRecipesAsync(string query)
    {
        await EnsureLoadedAsync();
        if (string.IsNullOrWhiteSpace(query)) return _recipes!;
        return _recipes!
            .Where(r => (r.Name ?? string.Empty).Contains(query, StringComparison.OrdinalIgnoreCase)
                        || (r.Category?.Name ?? string.Empty).Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private async Task EnsureLoadedAsync()
    {
        if (_recipes is not null) return;
        await _gate.WaitAsync();
        try
        {
            if (_recipes is not null) return;
            _recipes = await ReadAsync<List<RecipeData>>("Recipes.json") ?? new();
            _categories = await ReadAsync<List<CategoryData>>("categories.json") ?? new();
            _cookbooks = await ReadAsync<List<CookbookData>>("Cookbooks.json") ?? new();
            _users = await ReadAsync<List<UserData>>("Users.json") ?? new();
            _notifications = await ReadAsync<List<NotificationData>>("Notifications.json") ?? new();
            var savedR = await ReadAsync<List<string>>("SavedRecipes.json") ?? new();
            _savedRecipeIds = savedR
                .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToHashSet();
            var savedC = await ReadAsync<List<string>>("SavedCookbooks.json") ?? new();
            _savedCookbookIds = savedC
                .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToHashSet();

            // Mark IsFavorite based on saved recipe ids so existing UI bindings light up.
            foreach (var r in _recipes)
            {
                if (_savedRecipeIds.Contains(r.Id)) r.IsFavorite = true;
            }

            _currentUser = _users.FirstOrDefault() ?? new UserData
            {
                Id = Guid.NewGuid(),
                FullName = "Chef Andrea",
                Email = "andrea@unochefs.dev",
                PhoneNumber = "+1 (514) 555-0123",
                Followers = 120,
                Following = 78,
                Recipes = 12,
                Description = "Passionate about food and life",
                IsCurrent = true,
            };
            _currentUser.IsCurrent = true;
        }
        finally
        {
            _gate.Release();
        }
    }

    private static readonly string LogFile = Path.Combine(Path.GetTempPath(), "ChefsTest3-data.log");

    private static void Log(string msg)
    {
        try { File.AppendAllText(LogFile, $"[{DateTime.Now:HH:mm:ss.fff}] {msg}{Environment.NewLine}"); }
        catch { }
    }

    private static async Task<T?> ReadAsync<T>(string fileName) where T : class
    {
        try
        {
            var uri = new Uri($"ms-appx:///Assets/data/{fileName}");
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            using var stream = await file.OpenStreamForReadAsync();
            var data = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions);
            Log($"OK ms-appx {fileName}");
            return data;
        }
        catch (Exception ex)
        {
            Log($"FAIL ms-appx {fileName}: {ex.GetType().Name}: {ex.Message}");
        }
        try
        {
            var basePath = AppContext.BaseDirectory;
            var path = Path.Combine(basePath, "Assets", "data", fileName);
            Log($"Trying FS path={path} exists={File.Exists(path)}");
            if (File.Exists(path))
            {
                using var fs = File.OpenRead(path);
                var data = await JsonSerializer.DeserializeAsync<T>(fs, JsonOptions);
                Log($"OK FS {fileName} count-as-T={data?.GetType().Name}");
                return data;
            }
        }
        catch (Exception ex)
        {
            Log($"FAIL FS {fileName}: {ex.GetType().Name}: {ex.Message}");
        }
        return null;
    }
}
