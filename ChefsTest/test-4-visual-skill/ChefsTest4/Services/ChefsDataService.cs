using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ChefsTest4.Models;
using Windows.ApplicationModel;
using Windows.Storage;

namespace ChefsTest4.Services;

public sealed class ChefsDataService : IChefsDataService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private List<RecipeData> _recipes = new();
    private List<CategoryData> _categories = new();
    private List<UserData> _users = new();
    private List<CookbookData> _cookbooks = new();
    private List<NotificationData> _notifications = new();
    private HashSet<Guid> _savedRecipes = new();
    private HashSet<Guid> _savedCookbooks = new();
    private bool _initialized;
    private readonly object _gate = new();

    public UserData? CurrentUser { get; private set; }

    public bool NotificationsEnabled { get; set; } = true;

    public Guid? SelectedRecipeId { get; set; }
    public Guid? SelectedCookbookId { get; set; }
    public Guid? SelectedUserId { get; set; }

    private bool _nightMode;
    public bool NightModeEnabled
    {
        get => _nightMode;
        set
        {
            if (_nightMode != value)
            {
                _nightMode = value;
                NightModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public event EventHandler? FavoritesChanged;
    public event EventHandler? CookbooksChanged;
    public event EventHandler? NightModeChanged;

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _recipes = await LoadAsync<List<RecipeData>>("Recipes.json") ?? new();
        _categories = await LoadAsync<List<CategoryData>>("categories.json") ?? new();
        _users = await LoadAsync<List<UserData>>("Users.json") ?? new();
        _cookbooks = await LoadAsync<List<CookbookData>>("Cookbooks.json") ?? new();
        _notifications = await LoadAsync<List<NotificationData>>("Notifications.json") ?? new();
        var savedRecipeIds = await LoadAsync<List<string>>("SavedRecipes.json") ?? new();
        var savedCookbookIds = await LoadAsync<List<string>>("SavedCookbooks.json") ?? new();
        _savedRecipes = savedRecipeIds.Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty).Where(g => g != Guid.Empty).ToHashSet();
        _savedCookbooks = savedCookbookIds.Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty).Where(g => g != Guid.Empty).ToHashSet();
        _initialized = true;
    }

    private static async Task<T?> LoadAsync<T>(string fileName)
    {
        try
        {
            var uri = new Uri($"ms-appx:///Assets/Data/{fileName}");
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            using var stream = await file.OpenStreamForReadAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOpts);
        }
        catch
        {
            return default;
        }
    }

    public Task<IReadOnlyList<RecipeData>> GetAllRecipesAsync() => Task.FromResult<IReadOnlyList<RecipeData>>(_recipes.Select(WithFavorite).ToList());

    public Task<IReadOnlyList<RecipeData>> GetTrendingAsync() =>
        Task.FromResult<IReadOnlyList<RecipeData>>(_recipes.Take(8).Select(WithFavorite).ToList());

    public Task<IReadOnlyList<RecipeData>> GetPopularAsync() =>
        Task.FromResult<IReadOnlyList<RecipeData>>(_recipes.Skip(2).Take(8).Select(WithFavorite).ToList());

    public Task<IReadOnlyList<RecipeData>> GetRecentAsync() =>
        Task.FromResult<IReadOnlyList<RecipeData>>(_recipes.OrderByDescending(r => r.Date).Take(10).Select(WithFavorite).ToList());

    public Task<IReadOnlyList<RecipeData>> GetFavoritedAsync() =>
        Task.FromResult<IReadOnlyList<RecipeData>>(_recipes.Where(r => _savedRecipes.Contains(r.Id)).Select(WithFavorite).ToList());

    public Task<IReadOnlyList<CategoryData>> GetCategoriesAsync() => Task.FromResult<IReadOnlyList<CategoryData>>(_categories);
    public Task<IReadOnlyList<UserData>> GetUsersAsync() => Task.FromResult<IReadOnlyList<UserData>>(_users);
    public Task<IReadOnlyList<UserData>> GetPopularCreatorsAsync() =>
        Task.FromResult<IReadOnlyList<UserData>>(_users.OrderByDescending(u => u.Followers ?? 0).Take(8).ToList());

    public Task<IReadOnlyList<CookbookData>> GetCookbooksAsync() => Task.FromResult<IReadOnlyList<CookbookData>>(_cookbooks);

    public Task<IReadOnlyList<CookbookData>> GetSavedCookbooksAsync() =>
        Task.FromResult<IReadOnlyList<CookbookData>>(_cookbooks.Where(c => _savedCookbooks.Contains(c.Id)).ToList());

    public Task<IReadOnlyList<NotificationData>> GetNotificationsAsync() => Task.FromResult<IReadOnlyList<NotificationData>>(_notifications);

    public Task<RecipeData?> GetRecipeAsync(Guid id) => Task.FromResult(_recipes.FirstOrDefault(r => r.Id == id) is { } r ? WithFavorite(r) : null);

    public Task<UserData?> GetUserAsync(Guid id) => Task.FromResult<UserData?>(_users.FirstOrDefault(u => u.Id == id));

    public Task<UserData?> AuthenticateAsync(string email, string password)
    {
        var user = _users.FirstOrDefault(u =>
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(u.Password, password));
        return Task.FromResult(user);
    }

    public void SetCurrentUser(UserData user) => CurrentUser = user with { IsCurrent = true };

    public Task ToggleFavoriteAsync(Guid recipeId)
    {
        lock (_gate)
        {
            if (!_savedRecipes.Add(recipeId)) _savedRecipes.Remove(recipeId);
        }
        FavoritesChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public bool IsFavorite(Guid recipeId) => _savedRecipes.Contains(recipeId);

    public Task SaveCookbookAsync(CookbookData cookbook)
    {
        lock (_gate)
        {
            var existing = _cookbooks.FindIndex(c => c.Id == cookbook.Id);
            if (existing >= 0) _cookbooks[existing] = cookbook;
            else { _cookbooks.Add(cookbook); _savedCookbooks.Add(cookbook.Id); }
        }
        CookbooksChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task DeleteCookbookAsync(Guid cookbookId)
    {
        lock (_gate)
        {
            _cookbooks.RemoveAll(c => c.Id == cookbookId);
            _savedCookbooks.Remove(cookbookId);
        }
        CookbooksChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    private RecipeData WithFavorite(RecipeData r) => r with { IsFavorite = _savedRecipes.Contains(r.Id) };
}
