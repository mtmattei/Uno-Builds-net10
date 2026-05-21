using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ChefsTest1.Models;
using Windows.Storage;

namespace ChefsTest1.Services;

public interface IRecipeService
{
    Task<IList<Recipe>> GetAllAsync();
    Task<IList<Recipe>> GetTrendingAsync();
    Task<IList<Recipe>> GetPopularAsync();
    Task<IList<Recipe>> GetFavoritedAsync();
    Task<IList<Category>> GetCategoriesAsync();
    Task<Recipe?> GetByIdAsync(Guid id);
    Task ToggleFavoriteAsync(Guid recipeId);
}

public interface ICookbookService
{
    Task<IList<Cookbook>> GetAllAsync();
    Task<IList<Cookbook>> GetSavedAsync();
    Task<Cookbook?> GetByIdAsync(Guid id);
    Task SaveAsync(Cookbook cookbook);
}

public interface IUserService
{
    Task<User?> GetCurrentAsync();
    Task<IList<User>> GetPopularCreatorsAsync();
    Task<IList<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task<Guid?> AuthenticateAsync(string email, string password);
}

public interface INotificationService
{
    Task<IList<Notification>> GetAllAsync();
}

public interface IThemeServiceLocal
{
    bool IsDark { get; }
    void Toggle();
    event EventHandler? ThemeChanged;
}

public class DataLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    public static async Task<T?> LoadAsync<T>(string fileName)
    {
        try
        {
            var uri = new Uri($"ms-appx:///Assets/Data/{fileName}");
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            using var stream = await file.OpenStreamForReadAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream, Options);
        }
        catch (Exception)
        {
            return default;
        }
    }
}

public class RecipeService : IRecipeService
{
    private List<Recipe>? _cache;
    private List<Category>? _categoriesCache;
    private List<Guid>? _savedIds;
    private readonly object _lock = new();

    private async Task EnsureLoadedAsync()
    {
        if (_cache != null) return;
        var recipes = await DataLoader.LoadAsync<List<Recipe>>("Recipes.json") ?? new List<Recipe>();
        var categories = await DataLoader.LoadAsync<List<Category>>("categories.json") ?? new List<Category>();
        var saved = await DataLoader.LoadAsync<List<Guid>>("SavedRecipes.json") ?? new List<Guid>();
        lock (_lock)
        {
            _cache = recipes;
            _categoriesCache = categories;
            _savedIds = saved;
            foreach (var r in _cache) r.IsFavorite = _savedIds.Contains(r.Id);
            foreach (var c in _categoriesCache)
            {
                var n = _cache.Count(r => r.Category?.Id == c.Id);
                c.RecipesLabel = $"{n} recipes";
            }
        }
    }

    public async Task<IList<Recipe>> GetAllAsync()
    {
        await EnsureLoadedAsync();
        return _cache!.ToList();
    }

    public async Task<IList<Recipe>> GetTrendingAsync()
    {
        await EnsureLoadedAsync();
        return _cache!.Take(8).ToList();
    }

    public async Task<IList<Recipe>> GetPopularAsync()
    {
        await EnsureLoadedAsync();
        return _cache!.Skip(2).Take(8).ToList();
    }

    public async Task<IList<Recipe>> GetFavoritedAsync()
    {
        await EnsureLoadedAsync();
        return _cache!.Where(r => r.IsFavorite).ToList();
    }

    public async Task<IList<Category>> GetCategoriesAsync()
    {
        await EnsureLoadedAsync();
        return _categoriesCache!.ToList();
    }

    public async Task<Recipe?> GetByIdAsync(Guid id)
    {
        await EnsureLoadedAsync();
        return _cache!.FirstOrDefault(r => r.Id == id);
    }

    public async Task ToggleFavoriteAsync(Guid recipeId)
    {
        await EnsureLoadedAsync();
        var recipe = _cache!.FirstOrDefault(r => r.Id == recipeId);
        if (recipe == null) return;
        recipe.IsFavorite = !recipe.IsFavorite;
        if (recipe.IsFavorite) _savedIds!.Add(recipeId);
        else _savedIds!.Remove(recipeId);
    }
}

public class CookbookService : ICookbookService
{
    private List<Cookbook>? _cache;
    private List<Guid>? _saved;
    private readonly object _lock = new();

    private async Task EnsureLoadedAsync()
    {
        if (_cache != null) return;
        var cookbooks = await DataLoader.LoadAsync<List<Cookbook>>("Cookbooks.json") ?? new List<Cookbook>();
        var saved = await DataLoader.LoadAsync<List<Guid>>("SavedCookbooks.json") ?? new List<Guid>();
        lock (_lock)
        {
            _cache = cookbooks;
            _saved = saved;
        }
    }

    public async Task<IList<Cookbook>> GetAllAsync()
    {
        await EnsureLoadedAsync();
        return _cache!.ToList();
    }

    public async Task<IList<Cookbook>> GetSavedAsync()
    {
        await EnsureLoadedAsync();
        return _cache!.Where(c => _saved!.Contains(c.Id)).ToList();
    }

    public async Task<Cookbook?> GetByIdAsync(Guid id)
    {
        await EnsureLoadedAsync();
        return _cache!.FirstOrDefault(c => c.Id == id);
    }

    public async Task SaveAsync(Cookbook cookbook)
    {
        await EnsureLoadedAsync();
        var existing = _cache!.FirstOrDefault(c => c.Id == cookbook.Id);
        if (existing != null)
        {
            existing.Name = cookbook.Name;
            existing.Recipes = cookbook.Recipes;
        }
        else
        {
            if (cookbook.Id == Guid.Empty) cookbook.Id = Guid.NewGuid();
            _cache!.Add(cookbook);
        }
        if (!_saved!.Contains(cookbook.Id)) _saved!.Add(cookbook.Id);
    }
}

public class UserService : IUserService
{
    private List<User>? _cache;
    private User? _current;
    private readonly object _lock = new();

    private async Task EnsureLoadedAsync()
    {
        if (_cache != null) return;
        var users = await DataLoader.LoadAsync<List<User>>("Users.json") ?? new List<User>();
        lock (_lock)
        {
            _cache = users;
            _current = _cache.FirstOrDefault();
            if (_current != null) _current.IsCurrent = true;
        }
    }

    public async Task<User?> GetCurrentAsync()
    {
        await EnsureLoadedAsync();
        return _current ?? new User { FullName = "Jenna Smith", Email = "Jenna.Smith@platform.uno", Recipes = 12, Followers = 156, Following = 3127, UrlProfileImage = "ms-appx:///Assets/Profiles/niki_samantha.png" };
    }

    public async Task<IList<User>> GetPopularCreatorsAsync()
    {
        await EnsureLoadedAsync();
        return _cache!.Take(8).ToList();
    }

    public async Task<IList<User>> GetAllAsync()
    {
        await EnsureLoadedAsync();
        return _cache!.ToList();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        await EnsureLoadedAsync();
        return _cache!.FirstOrDefault(u => u.Id == id);
    }

    public async Task<Guid?> AuthenticateAsync(string email, string password)
    {
        await EnsureLoadedAsync();
        var match = _cache!.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase) && u.Password == password);
        return match?.Id;
    }
}

public class NotificationService : INotificationService
{
    private List<Notification>? _cache;

    public async Task<IList<Notification>> GetAllAsync()
    {
        if (_cache == null)
        {
            _cache = await DataLoader.LoadAsync<List<Notification>>("Notifications.json") ?? new List<Notification>();
        }
        return _cache.ToList();
    }
}

public class ThemeServiceLocal : IThemeServiceLocal
{
    private bool _isDark;
    public bool IsDark => _isDark;
    public event EventHandler? ThemeChanged;
    public void Toggle()
    {
        _isDark = !_isDark;
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }
}
