using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace ChefsTest5.Services;

public sealed class ChefsDataService : IChefsDataService
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private List<RecipeData>? _recipes;
    private List<CategoryData>? _categories;
    private List<CookbookData>? _cookbooks;
    private List<CookbookData>? _savedCookbooks;
    private List<UserData>? _users;
    private List<NotificationData>? _notifications;
    private HashSet<Guid> _favoriteIds = new();
    private bool _nightMode;

    public bool IsNightMode => _nightMode;
    public event EventHandler<bool>? NightModeChanged;

    private async Task<T> LoadAsync<T>(string fileName)
    {
        var uri = new Uri($"ms-appx:///Assets/Data/{fileName}");
        var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
        var text = await FileIO.ReadTextAsync(file);
        return JsonSerializer.Deserialize<T>(text, _options)!;
    }

    private async Task EnsureLoadedAsync()
    {
        if (_recipes is null)
        {
            _recipes = await LoadAsync<List<RecipeData>>("Recipes.json");
            foreach (var r in _recipes)
            {
                if (r.IsFavorite) _favoriteIds.Add(r.Id);
            }
        }
        _categories ??= await LoadAsync<List<CategoryData>>("categories.json");
        _cookbooks ??= await LoadAsync<List<CookbookData>>("Cookbooks.json");
        try { _savedCookbooks ??= await LoadAsync<List<CookbookData>>("SavedCookbooks.json"); }
        catch { _savedCookbooks ??= new(); }
        _users ??= await LoadAsync<List<UserData>>("Users.json");
        _notifications ??= await LoadAsync<List<NotificationData>>("Notifications.json");
    }

    public async Task<IReadOnlyList<RecipeData>> GetRecipesAsync()
    {
        await EnsureLoadedAsync();
        return _recipes!;
    }

    public async Task<IReadOnlyList<RecipeData>> GetTrendingAsync()
    {
        await EnsureLoadedAsync();
        return _recipes!.Take(6).ToList();
    }

    public async Task<IReadOnlyList<RecipeData>> GetPopularAsync()
    {
        await EnsureLoadedAsync();
        return _recipes!.Skip(2).Take(6).ToList();
    }

    public async Task<IReadOnlyList<RecipeData>> GetRecentAsync()
    {
        await EnsureLoadedAsync();
        return _recipes!.OrderByDescending(r => r.Date).Take(6).ToList();
    }

    public async Task<IReadOnlyList<RecipeData>> GetFavoritedAsync()
    {
        await EnsureLoadedAsync();
        return _recipes!.Where(r => _favoriteIds.Contains(r.Id)).ToList();
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
        return _savedCookbooks ?? new List<CookbookData>();
    }

    public async Task<IReadOnlyList<UserData>> GetUsersAsync()
    {
        await EnsureLoadedAsync();
        return _users!;
    }

    public async Task<IReadOnlyList<UserData>> GetPopularCreatorsAsync()
    {
        await EnsureLoadedAsync();
        return _users!.OrderByDescending(u => u.Followers ?? 0).Take(8).ToList();
    }

    public async Task<UserData?> GetCurrentUserAsync()
    {
        await EnsureLoadedAsync();
        return _users!.FirstOrDefault();
    }

    public async Task<IReadOnlyList<NotificationData>> GetNotificationsAsync()
    {
        await EnsureLoadedAsync();
        return _notifications!;
    }

    public async Task<RecipeData?> GetRecipeAsync(Guid id)
    {
        await EnsureLoadedAsync();
        return _recipes!.FirstOrDefault(r => r.Id == id);
    }

    public async Task ToggleFavoriteAsync(Guid recipeId)
    {
        await EnsureLoadedAsync();
        if (!_favoriteIds.Add(recipeId))
        {
            _favoriteIds.Remove(recipeId);
        }
        var recipe = _recipes!.FirstOrDefault(r => r.Id == recipeId);
        if (recipe is not null)
        {
            recipe.IsFavorite = _favoriteIds.Contains(recipeId);
        }
    }

    public Task SetNightModeAsync(bool enabled)
    {
        _nightMode = enabled;
        NightModeChanged?.Invoke(this, enabled);
        return Task.CompletedTask;
    }
}
