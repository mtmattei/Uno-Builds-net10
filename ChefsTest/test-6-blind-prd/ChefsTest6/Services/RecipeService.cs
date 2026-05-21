namespace ChefsTest6.Services;

public class RecipeService : IRecipeService
{
    private readonly List<RecipeData> _all;
    private readonly List<CategoryData> _categories;
    private readonly HashSet<Guid> _favorites;

    public RecipeService()
    {
        _all = DataLoader.Load<List<RecipeData>>("Recipes.json") ?? new();
        _categories = DataLoader.Load<List<CategoryData>>("categories.json") ?? new();

        // Stamp recipe count per category for the chip row
        foreach (var c in _categories)
        {
            c.RecipeCount = _all.Count(r => r.Category?.Id == c.Id);
        }

        // Seed initial favorites from SavedRecipes.json
        try
        {
            var saved = DataLoader.Load<List<RecipeData>>("SavedRecipes.json") ?? new();
            _favorites = new HashSet<Guid>(saved.Select(r => r.Id));
        }
        catch
        {
            _favorites = new HashSet<Guid>();
        }

        foreach (var r in _all)
        {
            r.IsFavorite = _favorites.Contains(r.Id);
        }
    }

    public IReadOnlyList<RecipeData> All() => _all;

    public IReadOnlyList<RecipeData> Trending() =>
        _all.OrderByDescending(r => (r.Reviews?.Count ?? 0))
            .ThenByDescending(r => r.Date)
            .Take(10).ToList();

    public IReadOnlyList<RecipeData> Popular() =>
        _all.OrderByDescending(r => r.Reviews?.Count ?? 0).Take(10).ToList();

    public IReadOnlyList<RecipeData> RecentlyAdded() =>
        _all.OrderByDescending(r => r.Date).Take(12).ToList();

    public IReadOnlyList<RecipeData> Favorites() =>
        _all.Where(r => r.IsFavorite).ToList();

    public IReadOnlyList<CategoryData> Categories() => _categories;

    public RecipeData? FindById(Guid id) => _all.FirstOrDefault(r => r.Id == id);

    public IReadOnlyList<RecipeData> Search(string? text, CategoryData? category, int? maxMinutes, int? difficulty)
    {
        IEnumerable<RecipeData> q = _all;

        if (!string.IsNullOrWhiteSpace(text))
        {
            var t = text.Trim();
            q = q.Where(r =>
                (r.Name ?? string.Empty).Contains(t, StringComparison.OrdinalIgnoreCase)
                || (r.Category?.Name ?? string.Empty).Contains(t, StringComparison.OrdinalIgnoreCase));
        }
        if (category is not null)
        {
            q = q.Where(r => r.Category?.Id == category.Id);
        }
        if (maxMinutes is int m && m > 0)
        {
            q = q.Where(r => r.CookTime.TotalMinutes <= m);
        }
        if (difficulty is int d && d > 0)
        {
            q = q.Where(r => r.Difficulty == d);
        }

        return q.ToList();
    }

    public void ToggleFavorite(RecipeData recipe)
    {
        if (recipe.IsFavorite)
        {
            recipe.IsFavorite = false;
            _favorites.Remove(recipe.Id);
        }
        else
        {
            recipe.IsFavorite = true;
            _favorites.Add(recipe.Id);
        }
    }
}
