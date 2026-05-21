using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ChefsTest7.Models;
using Windows.Storage;
using Windows.ApplicationModel;

namespace ChefsTest7.Services;

internal sealed class FlexibleTimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            return string.IsNullOrEmpty(s) ? TimeSpan.Zero : TimeSpan.Parse(s);
        }
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            long ticks = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType == JsonTokenType.PropertyName && string.Equals(reader.GetString(), "ticks", StringComparison.OrdinalIgnoreCase))
                {
                    reader.Read();
                    if (reader.TokenType == JsonTokenType.Number) ticks = reader.GetInt64();
                }
            }
            return TimeSpan.FromTicks(ticks);
        }
        if (reader.TokenType == JsonTokenType.Number)
        {
            return TimeSpan.FromTicks(reader.GetInt64());
        }
        return TimeSpan.Zero;
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}

public interface IChefsDataService
{
    Task<IReadOnlyList<RecipeData>> GetRecipesAsync();
    Task<IReadOnlyList<RecipeData>> GetTrendingAsync();
    Task<IReadOnlyList<RecipeData>> GetPopularAsync();
    Task<IReadOnlyList<RecipeData>> GetFavoritedAsync();
    Task<IReadOnlyList<CategoryData>> GetCategoriesAsync();
    Task<IReadOnlyList<CookbookData>> GetCookbooksAsync();
    Task<IReadOnlyList<CookbookData>> GetSavedCookbooksAsync();
    Task<IReadOnlyList<UserData>> GetUsersAsync();
    Task<UserData?> GetCurrentUserAsync();
    Task<IReadOnlyList<UserData>> GetPopularCreatorsAsync();
    Task<IReadOnlyList<NotificationData>> GetNotificationsAsync();
    Task ToggleFavoriteAsync(Guid recipeId);
    Task<RecipeData?> GetRecipeAsync(Guid id);
    Task SaveCookbookAsync(CookbookData cookbook);
    Task UpdateCurrentUserAsync(UserData user);
}

public class ChefsDataService : IChefsDataService
{
    private readonly JsonSerializerOptions _opts = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new FlexibleTimeSpanConverter() }
    };

    private List<RecipeData>? _recipes;
    private List<CategoryData>? _categories;
    private List<CookbookData>? _cookbooks;
    private List<CookbookData>? _savedCookbooks;
    private List<UserData>? _users;
    private List<NotificationData>? _notifications;

    private async Task<string> ReadAssetAsync(string relativePath)
    {
        var uri = new Uri($"ms-appx:///Assets/Data/{relativePath}");
        var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
        return await FileIO.ReadTextAsync(file);
    }

    private async Task EnsureLoadedAsync()
    {
        if (_recipes is null)
        {
            try
            {
                var json = await ReadAssetAsync("Recipes.json");
                _recipes = JsonSerializer.Deserialize<List<RecipeData>>(json, _opts) ?? new();
            }
            catch { _recipes = new(); }

            // Inject default Nutrition values where missing for the donut chart on the Nutrition tab
            foreach (var r in _recipes)
            {
                r.Nutrition ??= new NutritionData
                {
                    Protein = 35, ProteinBase = 100,
                    Carbs = 45, CarbsBase = 100,
                    Fat = 20, FatBase = 100
                };
                if (string.IsNullOrEmpty(r.Calories)) r.Calories = "250 kcal";
            }
        }
        if (_categories is null)
        {
            try
            {
                var json = await ReadAssetAsync("categories.json");
                _categories = JsonSerializer.Deserialize<List<CategoryData>>(json, _opts) ?? new();
            }
            catch { _categories = new(); }
        }
        if (_cookbooks is null)
        {
            try
            {
                var json = await ReadAssetAsync("Cookbooks.json");
                _cookbooks = JsonSerializer.Deserialize<List<CookbookData>>(json, _opts) ?? new();
            }
            catch { _cookbooks = new(); }
        }
        if (_savedCookbooks is null)
        {
            try
            {
                var json = await ReadAssetAsync("SavedCookbooks.json");
                _savedCookbooks = JsonSerializer.Deserialize<List<CookbookData>>(json, _opts) ?? new();
            }
            catch { _savedCookbooks = new(); }
        }
        if (_users is null)
        {
            try
            {
                var json = await ReadAssetAsync("Users.json");
                _users = JsonSerializer.Deserialize<List<UserData>>(json, _opts) ?? new();
                if (_users.Count > 0) _users[0].IsCurrent = true;
            }
            catch { _users = new(); }
        }
        if (_notifications is null)
        {
            try
            {
                var json = await ReadAssetAsync("Notifications.json");
                _notifications = JsonSerializer.Deserialize<List<NotificationData>>(json, _opts) ?? new();
            }
            catch { _notifications = new(); }
        }
    }

    public async Task<IReadOnlyList<RecipeData>> GetRecipesAsync()
    { await EnsureLoadedAsync(); return _recipes!; }

    public async Task<IReadOnlyList<RecipeData>> GetTrendingAsync()
    { await EnsureLoadedAsync(); return _recipes!.Take(6).ToList(); }

    public async Task<IReadOnlyList<RecipeData>> GetPopularAsync()
    { await EnsureLoadedAsync(); return _recipes!.Skip(2).Take(8).ToList(); }

    public async Task<IReadOnlyList<RecipeData>> GetFavoritedAsync()
    { await EnsureLoadedAsync(); return _recipes!.Where(r => r.IsFavorite || r.Save).ToList(); }

    public async Task<IReadOnlyList<CategoryData>> GetCategoriesAsync()
    { await EnsureLoadedAsync(); return _categories!; }

    public async Task<IReadOnlyList<CookbookData>> GetCookbooksAsync()
    { await EnsureLoadedAsync(); return _cookbooks!; }

    public async Task<IReadOnlyList<CookbookData>> GetSavedCookbooksAsync()
    { await EnsureLoadedAsync(); return _savedCookbooks!; }

    public async Task<IReadOnlyList<UserData>> GetUsersAsync()
    { await EnsureLoadedAsync(); return _users!; }

    public async Task<UserData?> GetCurrentUserAsync()
    { await EnsureLoadedAsync(); return _users!.FirstOrDefault(u => u.IsCurrent) ?? _users!.FirstOrDefault(); }

    public async Task<IReadOnlyList<UserData>> GetPopularCreatorsAsync()
    { await EnsureLoadedAsync(); return _users!.Skip(1).Take(8).ToList(); }

    public async Task<IReadOnlyList<NotificationData>> GetNotificationsAsync()
    { await EnsureLoadedAsync(); return _notifications!; }

    public async Task ToggleFavoriteAsync(Guid recipeId)
    {
        await EnsureLoadedAsync();
        var r = _recipes!.FirstOrDefault(x => x.Id == recipeId);
        if (r is not null) r.IsFavorite = !r.IsFavorite;
    }

    public async Task<RecipeData?> GetRecipeAsync(Guid id)
    {
        await EnsureLoadedAsync();
        return _recipes!.FirstOrDefault(r => r.Id == id);
    }

    public async Task SaveCookbookAsync(CookbookData cookbook)
    {
        await EnsureLoadedAsync();
        var existing = _cookbooks!.FirstOrDefault(c => c.Id == cookbook.Id);
        if (existing is null) _cookbooks!.Add(cookbook);
        else
        {
            existing.Name = cookbook.Name;
            existing.Recipes = cookbook.Recipes;
        }
    }

    public async Task UpdateCurrentUserAsync(UserData user)
    {
        await EnsureLoadedAsync();
        var existing = _users!.FirstOrDefault(u => u.Id == user.Id);
        if (existing is not null)
        {
            existing.FullName = user.FullName;
            existing.Email = user.Email;
            existing.PhoneNumber = user.PhoneNumber;
        }
    }
}
