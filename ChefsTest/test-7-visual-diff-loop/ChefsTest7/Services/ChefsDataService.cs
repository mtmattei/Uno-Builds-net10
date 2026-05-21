using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ChefsTest7.Models;
using Windows.Storage;

namespace ChefsTest7.Services;

public sealed class ChefsDataService : IChefsDataService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.General)
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private List<Recipe> _recipes = new();
    private List<Category> _categories = new();
    private List<User> _users = new();
    private List<Cookbook> _cookbooks = new();
    private List<Notification> _notifications = new();
    private HashSet<string> _savedRecipeIds = new();
    private HashSet<string> _savedCookbookIds = new();
    private bool _initialized;

    public IReadOnlyList<Recipe> Recipes => _recipes;
    public IReadOnlyList<Category> Categories => _categories;
    public IReadOnlyList<User> Users => _users;
    public IReadOnlyList<Cookbook> Cookbooks => _cookbooks;
    public IReadOnlyList<Notification> Notifications => _notifications;
    public HashSet<string> SavedRecipeIds => _savedRecipeIds;
    public HashSet<string> SavedCookbookIds => _savedCookbookIds;

    public User? CurrentUser { get; private set; }

    public void SetCurrentUser(User user) => CurrentUser = user;

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        _recipes = await LoadAsync<List<Recipe>>("Recipes.json") ?? new();
        _categories = await LoadAsync<List<Category>>("categories.json") ?? new();
        _users = await LoadAsync<List<User>>("Users.json") ?? new();
        _cookbooks = await LoadAsync<List<Cookbook>>("Cookbooks.json") ?? new();
        _notifications = await LoadAsync<List<Notification>>("Notifications.json") ?? new();

        var savedRecipes = await LoadAsync<List<SavedRecipe>>("SavedRecipes.json") ?? new();
        var savedCookbooks = await LoadAsync<List<SavedCookbook>>("SavedCookbooks.json") ?? new();

        _savedRecipeIds = savedRecipes.Select(s => s.RecipeId).Where(s => !string.IsNullOrEmpty(s)).ToHashSet();
        _savedCookbookIds = savedCookbooks.Select(s => s.CookbookId).Where(s => !string.IsNullOrEmpty(s)).ToHashSet();

        if (CurrentUser is null && _users.Count > 0)
        {
            CurrentUser = _users.FirstOrDefault(u => u.FullName == "James Bondi") ?? _users[0];
        }
    }

    public bool ToggleSavedRecipe(string recipeId)
    {
        if (_savedRecipeIds.Contains(recipeId))
        {
            _savedRecipeIds.Remove(recipeId);
            return false;
        }
        _savedRecipeIds.Add(recipeId);
        return true;
    }

    public bool ToggleSavedCookbook(string cookbookId)
    {
        if (_savedCookbookIds.Contains(cookbookId))
        {
            _savedCookbookIds.Remove(cookbookId);
            return false;
        }
        _savedCookbookIds.Add(cookbookId);
        return true;
    }

    public void AddCookbook(Cookbook cookbook) => _cookbooks.Add(cookbook);

    public void UpdateCookbook(Cookbook cookbook)
    {
        var idx = _cookbooks.FindIndex(c => c.Id == cookbook.Id);
        if (idx >= 0) _cookbooks[idx] = cookbook;
        else _cookbooks.Add(cookbook);
    }

    private static async Task<T?> LoadAsync<T>(string fileName) where T : class
    {
        try
        {
            var uri = new Uri($"ms-appx:///Assets/Data/{fileName}");
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            using var stream = await file.OpenStreamForReadAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOpts);
        }
        catch (Exception)
        {
            return default;
        }
    }
}
