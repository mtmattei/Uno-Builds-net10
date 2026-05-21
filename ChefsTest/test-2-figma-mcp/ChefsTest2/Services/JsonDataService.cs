using System.Text.Json;
using ChefsTest2.Models;
using Windows.Storage;

namespace ChefsTest2.Services;

public sealed class JsonDataService : IDataService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private readonly Lazy<Task<IReadOnlyList<Recipe>>> _recipes;
    private readonly Lazy<Task<IReadOnlyList<Category>>> _categories;
    private readonly Lazy<Task<IReadOnlyList<Cookbook>>> _cookbooks;
    private readonly Lazy<Task<IReadOnlyList<User>>> _users;
    private readonly Lazy<Task<IReadOnlyList<Notification>>> _notifications;
    private readonly Lazy<Task<IReadOnlyList<string>>> _savedRecipeIds;
    private readonly Lazy<Task<IReadOnlyList<string>>> _savedCookbookIds;

    public User CurrentUser { get; private set; } = new();
    public HashSet<string> FavoriteRecipeIds { get; } = new();
    public HashSet<string> SavedCookbookIds { get; } = new();

    public JsonDataService()
    {
        _recipes = new(() => LoadListAsync<Recipe>("Recipes.json"));
        _categories = new(() => LoadListAsync<Category>("categories.json"));
        _cookbooks = new(() => LoadListAsync<Cookbook>("Cookbooks.json"));
        _users = new(() => LoadListAsync<User>("Users.json"));
        _notifications = new(() => LoadListAsync<Notification>("Notifications.json"));
        _savedRecipeIds = new(() => LoadListAsync<string>("SavedRecipes.json"));
        _savedCookbookIds = new(() => LoadListAsync<string>("SavedCookbooks.json"));
    }

    private static async Task<IReadOnlyList<T>> LoadListAsync<T>(string fileName)
    {
        try
        {
            var uri = new Uri($"ms-appx:///Assets/Data/{fileName}");
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri).AsTask();
            using var stream = await file.OpenStreamForReadAsync();
            var list = await JsonSerializer.DeserializeAsync<List<T>>(stream, JsonOptions);
            return list ?? new List<T>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[JsonDataService] Failed to load {fileName}: {ex}");
            return new List<T>();
        }
    }

    public async Task<IReadOnlyList<Recipe>> GetRecipesAsync(CancellationToken ct = default)
        => await _recipes.Value;

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken ct = default)
        => await _categories.Value;

    public async Task<IReadOnlyList<Cookbook>> GetCookbooksAsync(CancellationToken ct = default)
        => await _cookbooks.Value;

    public async Task<IReadOnlyList<User>> GetUsersAsync(CancellationToken ct = default)
    {
        var users = await _users.Value;
        if (users.Count > 0 && string.IsNullOrEmpty(CurrentUser.Id))
        {
            CurrentUser = users[0];
        }
        return users;
    }

    public async Task<IReadOnlyList<Notification>> GetNotificationsAsync(CancellationToken ct = default)
        => await _notifications.Value;

    public async Task<IReadOnlyList<string>> GetSavedRecipeIdsAsync(CancellationToken ct = default)
    {
        var ids = await _savedRecipeIds.Value;
        if (FavoriteRecipeIds.Count == 0)
        {
            foreach (var id in ids) FavoriteRecipeIds.Add(id);
        }
        return ids;
    }

    public async Task<IReadOnlyList<string>> GetSavedCookbookIdsAsync(CancellationToken ct = default)
    {
        var ids = await _savedCookbookIds.Value;
        if (SavedCookbookIds.Count == 0)
        {
            foreach (var id in ids) SavedCookbookIds.Add(id);
        }
        return ids;
    }

    public void ToggleFavoriteRecipe(string id)
    {
        if (!FavoriteRecipeIds.Add(id)) FavoriteRecipeIds.Remove(id);
    }

    public void ToggleSavedCookbook(string id)
    {
        if (!SavedCookbookIds.Add(id)) SavedCookbookIds.Remove(id);
    }
}
