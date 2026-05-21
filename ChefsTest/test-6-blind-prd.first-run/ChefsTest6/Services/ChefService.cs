using System.Collections.ObjectModel;

namespace ChefsTest6.Services;

public sealed class ChefService : IChefService
{
    private readonly List<Recipe> _recipes = new();
    private readonly List<Category> _categories = new();
    private readonly ObservableCollection<Cookbook> _cookbooks = new();
    private readonly ObservableCollection<Cookbook> _savedCookbooks = new();
    private readonly List<User> _users = new();
    private readonly List<Notification> _notifications = new();
    private readonly HashSet<Guid> _favorites = new();
    private bool _initialized;

    public IReadOnlyList<Recipe> AllRecipes => _recipes;
    public IReadOnlyList<Category> Categories => _categories;
    public IReadOnlyList<User> AllUsers => _users;
    public IReadOnlyList<Notification> Notifications => _notifications;
    public IReadOnlyList<Cookbook> AllCookbooks => _cookbooks;
    public IReadOnlyList<Cookbook> SavedCookbooks => _savedCookbooks;

    public IReadOnlyList<Recipe> Trending =>
        _recipes.OrderByDescending(r => r.Reviews?.Count ?? 0).Take(8).ToList();

    public IReadOnlyList<Recipe> Popular =>
        _recipes.OrderByDescending(r => r.Reviews?.Sum(rv => rv.Likes.Count) ?? 0).Take(8).ToList();

    public IReadOnlyList<Recipe> RecentlyAdded =>
        _recipes.OrderByDescending(r => r.Date).Take(8).ToList();

    public IReadOnlyList<Recipe> Favorites =>
        _recipes.Where(r => r.IsFavorite).ToList();

    public IReadOnlyList<User> PopularContributors =>
        _users.OrderByDescending(u => u.Followers ?? 0).Take(8).ToList();

    public User? CurrentUser { get; private set; }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        var rawRecipes = await FixtureLoader.LoadAsync<List<FixtureRecipe>>("Recipes.json") ?? new();
        var rawCategories = await FixtureLoader.LoadAsync<List<Category>>("categories.json") ?? new();
        var rawUsers = await FixtureLoader.LoadAsync<List<FixtureUser>>("Users.json") ?? new();
        var rawCookbooks = await FixtureLoader.LoadAsync<List<FixtureCookbook>>("Cookbooks.json") ?? new();
        var rawNotifications = await FixtureLoader.LoadAsync<List<FixtureNotification>>("Notifications.json") ?? new();
        var savedRecipeIds = await FixtureLoader.LoadAsync<List<Guid>>("SavedRecipes.json") ?? new();
        var savedCookbookIds = await FixtureLoader.LoadAsync<List<Guid>>("SavedCookbooks.json") ?? new();

        _categories.AddRange(rawCategories);
        _users.AddRange(rawUsers.Select(MapUser));

        foreach (var id in savedRecipeIds) _favorites.Add(id);

        // Build the master recipe list.
        var byId = new Dictionary<Guid, Recipe>();
        foreach (var fr in rawRecipes)
        {
            var recipe = MapRecipe(fr);
            if (!byId.ContainsKey(recipe.Id))
            {
                byId[recipe.Id] = recipe;
                _recipes.Add(recipe);
            }
        }

        // Cookbooks may reference recipes not present in master list — bring them in too,
        // but prefer the master record so favorite state stays unified.
        foreach (var fc in rawCookbooks)
        {
            var cookbook = new Cookbook
            {
                Id = fc.Id,
                UserId = fc.UserId,
                Name = fc.Name,
            };
            if (fc.Recipes != null)
            {
                foreach (var fr in fc.Recipes)
                {
                    if (!byId.TryGetValue(fr.Id, out var rec))
                    {
                        rec = MapRecipe(fr);
                        byId[rec.Id] = rec;
                        _recipes.Add(rec);
                    }
                    cookbook.Recipes.Add(rec);
                }
            }
            cookbook.RaiseCovers();
            _cookbooks.Add(cookbook);
            if (savedCookbookIds.Contains(cookbook.Id))
            {
                _savedCookbooks.Add(cookbook);
            }
        }

        foreach (var id in _favorites)
        {
            if (byId.TryGetValue(id, out var rec)) rec.IsFavorite = true;
        }

        foreach (var fn in rawNotifications)
        {
            _notifications.Add(new Notification
            {
                Title = fn.Title,
                Description = fn.Description,
                Date = fn.Date,
                IsRead = fn.IsRead,
            });
        }
    }

    public Recipe? GetRecipe(Guid id) => _recipes.FirstOrDefault(r => r.Id == id);

    public IReadOnlyList<Review> GetReviews(Guid recipeId) =>
        _recipes.FirstOrDefault(r => r.Id == recipeId)?.Reviews?.ToList() ?? new List<Review>();

    public void ToggleFavorite(Recipe recipe)
    {
        recipe.IsFavorite = !recipe.IsFavorite;
        if (recipe.IsFavorite) _favorites.Add(recipe.Id);
        else _favorites.Remove(recipe.Id);
    }

    public void ToggleLike(Review review)
    {
        var uid = CurrentUser?.Id ?? Guid.Empty;
        if (review.Likes.Contains(uid))
        {
            review.Likes.Remove(uid);
            review.UserLike = null;
        }
        else
        {
            review.Dislikes.Remove(uid);
            review.Likes.Add(uid);
            review.UserLike = true;
        }
        review.RaiseCounts();
    }

    public void ToggleDislike(Review review)
    {
        var uid = CurrentUser?.Id ?? Guid.Empty;
        if (review.Dislikes.Contains(uid))
        {
            review.Dislikes.Remove(uid);
            review.UserLike = null;
        }
        else
        {
            review.Likes.Remove(uid);
            review.Dislikes.Add(uid);
            review.UserLike = false;
        }
        review.RaiseCounts();
    }

    public IReadOnlyList<Recipe> Search(string? query, SearchFilters filters)
    {
        IEnumerable<Recipe> source = _recipes;
        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim();
            source = source.Where(r =>
                (r.Name?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (r.Category?.Name?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (filters is not null && filters.IsActive)
        {
            source = filters.CookingTime switch
            {
                FilterCookingTime.Under15 => source.Where(r => r.CookTime <= TimeSpan.FromMinutes(15)),
                FilterCookingTime.Under30 => source.Where(r => r.CookTime <= TimeSpan.FromMinutes(30)),
                FilterCookingTime.Under60 => source.Where(r => r.CookTime <= TimeSpan.FromMinutes(60)),
                _ => source,
            };

            source = filters.Skill switch
            {
                FilterSkill.Beginner => source.Where(r => r.Difficulty == 0),
                FilterSkill.Intermediate => source.Where(r => r.Difficulty == 1),
                FilterSkill.Advanced => source.Where(r => r.Difficulty >= 2),
                _ => source,
            };

            source = filters.Category switch
            {
                FilterCategory.Popular => source.OrderByDescending(r => r.Reviews?.Sum(rv => rv.Likes.Count) ?? 0),
                FilterCategory.Trending => source.OrderByDescending(r => r.Reviews?.Count ?? 0),
                FilterCategory.Recent => source.OrderByDescending(r => r.Date),
                _ => source,
            };
        }

        return source.ToList();
    }

    public Cookbook CreateCookbook(string name, IEnumerable<Recipe> recipes)
    {
        var book = new Cookbook
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUser?.Id ?? Guid.Empty,
            Name = name,
        };
        foreach (var r in recipes) book.Recipes.Add(r);
        book.RaiseCovers();
        _cookbooks.Add(book);
        return book;
    }

    public void UpdateCookbook(Cookbook cookbook, string name, IEnumerable<Recipe> recipes)
    {
        cookbook.Name = name;
        cookbook.Recipes.Clear();
        foreach (var r in recipes) cookbook.Recipes.Add(r);
        cookbook.RaiseCovers();
    }

    public Task<User?> AuthenticateAsync(LoginRequest request)
    {
        // PRD §3.2 mocked auth: any non-empty username + password lands the demo user.
        // Fixture lookup first; otherwise default to first user.
        var match = _users.FirstOrDefault(u =>
            string.Equals(u.Email, request.Email, StringComparison.OrdinalIgnoreCase) &&
            u.Password == request.Password);
        var user = match ?? (_users.Count > 0 ? _users[0] : null);
        if (user is not null)
        {
            CurrentUser = user with { IsCurrent = true };
            ApplyFavoritesToCurrentUser();
        }
        return Task.FromResult(CurrentUser);
    }

    private void ApplyFavoritesToCurrentUser()
    {
        // Already loaded from SavedRecipes.json; nothing else to do.
    }

    public User? GetUser(Guid id) => _users.FirstOrDefault(u => u.Id == id);

    public void UpdateCurrentUser(User updated)
    {
        CurrentUser = updated;
    }

    public IReadOnlyList<Recipe> GetRecipesByAuthor(Guid userId) =>
        _recipes.Where(r => r.UserId == userId).ToList();

    private static User MapUser(FixtureUser u) => new()
    {
        Id = u.Id,
        UrlProfileImage = u.UrlProfileImage,
        FullName = u.FullName,
        Description = u.Description,
        Email = u.Email,
        PhoneNumber = u.PhoneNumber,
        Password = u.Password,
        Followers = u.Followers,
        Following = u.Following,
        Recipes = u.Recipes,
    };

    private static Recipe MapRecipe(FixtureRecipe r) => new()
    {
        Id = r.Id,
        UserId = r.UserId,
        Name = r.Name,
        ImageUrl = r.ImageUrl,
        Serves = r.Serves,
        CookTime = r.CookTime,
        Difficulty = r.Difficulty,
        Calories = r.Calories,
        Details = r.Details,
        Date = r.Date,
        Category = r.Category,
        Creator = r.Creator is null ? null : MapUser(new FixtureUser
        {
            Id = r.Creator.Id,
            UrlProfileImage = r.Creator.UrlProfileImage,
            FullName = r.Creator.FullName,
            Description = r.Creator.Description,
            Email = r.Creator.Email,
            PhoneNumber = r.Creator.PhoneNumber,
            Password = r.Creator.Password,
            Followers = r.Creator.Followers,
            Following = r.Creator.Following,
            Recipes = r.Creator.Recipes,
        }),
        Nutrition = r.Nutrition ?? new Nutrition
        {
            Protein = 30, ProteinBase = 110,
            Carbs = 70, CarbsBase = 200,
            Fat = 20, FatBase = 70,
        },
        Ingredients = r.Ingredients?
            .Select(i => new Ingredient { UrlIcon = i.UrlIcon, Name = i.Name, Quantity = i.Quantity })
            .ToArray() ?? Array.Empty<Ingredient>(),
        Steps = r.Steps?
            .Select(s => new CookingStep
            {
                UrlVideo = s.UrlVideo,
                Name = s.Name,
                Number = s.Number,
                CookTime = s.CookTime,
                Cookware = s.Cookware,
                Ingredients = s.Ingredients,
                Description = s.Description,
            })
            .ToArray() ?? Array.Empty<CookingStep>(),
        Reviews = r.Reviews?
            .Select(MapReview)
            .ToArray() ?? Array.Empty<Review>(),
        IsFavorite = r.Save,
    };

    private static Review MapReview(FixtureReview r)
    {
        var review = new Review
        {
            Id = r.Id,
            RecipeId = r.RecipeId,
            UrlAuthorImage = r.UrlAuthorImage,
            CreatedBy = r.CreatedBy,
            PublisherName = r.PublisherName,
            Date = r.Date,
            Description = r.Description,
        };
        if (r.Likes != null) foreach (var id in r.Likes) review.Likes.Add(id);
        if (r.Dislikes != null) foreach (var id in r.Dislikes) review.Dislikes.Add(id);
        return review;
    }
}
