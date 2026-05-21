using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ChefsTest4.Models;
using ChefsTest4.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest4.ViewModels;

// Selected-id state passed via IChefsDataService (Selected{Recipe,Cookbook,User}Id) — set before NavigateRouteAsync, read in VM ctor.


// === AUTH ===

public partial class LoginViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private string email = "james.bondi@gmail.com";
    [ObservableProperty] private string password = "123";
    [ObservableProperty] private bool rememberMe = true;
    [ObservableProperty] private string? errorMessage;

    public LoginViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
    }

    [RelayCommand]
    private async Task LogInAsync()
    {
        await _data.InitializeAsync();
        var user = await _data.AuthenticateAsync(Email, Password);
        if (user is null)
        {
            ErrorMessage = "Invalid email or password.";
            return;
        }
        ErrorMessage = null;
        _data.SetCurrentUser(user);
        try { await _navigator.NavigateRouteAsync(this, "MainShell"); }
        catch { await _navigator.NavigateRouteAsync(this, "/MainShell"); }
    }

    [RelayCommand]
    private async Task RegisterAsync() => await _navigator.NavigateRouteAsync(this, "Register");

    [RelayCommand] private Task SignInAppleAsync() => LogInAsync();
    [RelayCommand] private Task SignInGoogleAsync() => LogInAsync();

    [RelayCommand]
    private void ForgotPassword() { ErrorMessage = "Password reset is not available in this preview."; }
}

public partial class RegisterViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string? errorMessage;

    public RegisterViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
    }

    [RelayCommand]
    private async Task SignUpAsync()
    {
        await _data.InitializeAsync();
        var user = (await _data.GetUsersAsync()).FirstOrDefault();
        if (user is not null) _data.SetCurrentUser(user);
        await _navigator.NavigateRouteAsync(this, "/MainShell");
    }

    [RelayCommand]
    private async Task LoginAsync() => await _navigator.NavigateRouteAsync(this, "MainShell");
}

// === ONBOARDING ===

public partial class OnboardingViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty] private int selectedIndex;
    public IReadOnlyList<OnboardingFrame> Frames { get; } = new[]
    {
        new OnboardingFrame("ms-appx:///Assets/Welcome/first_splash_screen.png", "Welcome to Uno Chefs!", "Discover delicious recipes from chefs around the world."),
        new OnboardingFrame("ms-appx:///Assets/Welcome/second_splash_screen.png", "Cook With Confidence", "Step-by-step video guidance keeps you on track in the kitchen."),
        new OnboardingFrame("ms-appx:///Assets/Welcome/third_splash_screen.png", "Save and Share", "Build cookbooks, follow chefs, and never lose a favorite recipe."),
    };

    public OnboardingViewModel(INavigator navigator) { _navigator = navigator; }

    public bool IsLastFrame => SelectedIndex >= Frames.Count - 1;
    partial void OnSelectedIndexChanged(int value) => OnPropertyChanged(nameof(IsLastFrame));

    [RelayCommand]
    private void Next()
    {
        if (SelectedIndex < Frames.Count - 1) SelectedIndex++;
        else _ = SkipAsync();
    }

    [RelayCommand] private void Previous() { if (SelectedIndex > 0) SelectedIndex--; }

    [RelayCommand]
    private async Task SkipAsync() => await _navigator.NavigateRouteAsync(this, "/Login");
}

public sealed record OnboardingFrame(string ImageUrl, string Title, string Body);

// === SHELL / MAIN ===

public partial class MainShellViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public HomeViewModel Home { get; }
    public SearchViewModel Search { get; }
    public FavoritesViewModel Favorites { get; }

    public MainShellViewModel(INavigator navigator, HomeViewModel home, SearchViewModel search, FavoritesViewModel favorites)
    {
        _navigator = navigator;
        Home = home;
        Search = search;
        Favorites = favorites;
    }
}

// === HOME ===

public partial class HomeViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private ObservableCollection<RecipeData> trending = new();
    [ObservableProperty] private ObservableCollection<RecipeData> recent = new();
    [ObservableProperty] private ObservableCollection<CategoryData> categories = new();
    [ObservableProperty] private ObservableCollection<UserData> contributors = new();

    public HomeViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await _data.InitializeAsync();
        ReplaceAll(Trending, await _data.GetTrendingAsync());
        ReplaceAll(Recent, await _data.GetRecentAsync());
        ReplaceAll(Categories, await _data.GetCategoriesAsync());
        ReplaceAll(Contributors, await _data.GetPopularCreatorsAsync());
    }

    private static void ReplaceAll<T>(ObservableCollection<T> target, IReadOnlyList<T> source)
    {
        target.Clear();
        foreach (var item in source) target.Add(item);
    }

    [RelayCommand]
    private async Task OpenRecipeAsync(RecipeData? r)
    {
        if (r is null) return;
        _data.SelectedRecipeId = r.Id;
        await _navigator.NavigateRouteAsync(this, "RecipeDetail");
    }

    [RelayCommand]
    private async Task OpenContributorAsync(UserData? u)
    {
        if (u is null) return;
        _data.SelectedUserId = u.Id;
        await _navigator.NavigateRouteAsync(this, "/OtherProfile");
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync(RecipeData? r)
    {
        if (r is null) return;
        await _data.ToggleFavoriteAsync(r.Id);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task NearMeAsync() => await _navigator.NavigateRouteAsync(this, "/NearMe");

    [RelayCommand]
    private async Task NotificationsAsync() => await _navigator.NavigateRouteAsync(this, "/Notifications");

    [RelayCommand]
    private async Task ProfileAsync() => await _navigator.NavigateRouteAsync(this, "/Profile");
}

// === SEARCH ===

public partial class SearchViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;
    private IReadOnlyList<RecipeData> _all = Array.Empty<RecipeData>();

    [ObservableProperty] private string queryText = string.Empty;
    [ObservableProperty] private ObservableCollection<RecipeData> results = new();

    public int ResultCount => Results.Count;
    public bool HasResults => Results.Count > 0;

    public SearchViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        Results.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(ResultCount));
            OnPropertyChanged(nameof(HasResults));
        };
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await _data.InitializeAsync();
        _all = await _data.GetAllRecipesAsync();
        ApplyFilter();
    }

    partial void OnQueryTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        Results.Clear();
        var q = QueryText?.Trim() ?? string.Empty;
        var matched = string.IsNullOrEmpty(q)
            ? _all
            : _all.Where(r => (r.Name ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var r in matched) Results.Add(r);
    }

    [RelayCommand]
    private async Task FiltersAsync() => await _navigator.NavigateRouteAsync(this, "/Filters");

    [RelayCommand]
    private async Task OpenRecipeAsync(RecipeData? r)
    {
        if (r is null) return;
        _data.SelectedRecipeId = r.Id;
        await _navigator.NavigateRouteAsync(this, "RecipeDetail");
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync(RecipeData? r)
    {
        if (r is null) return;
        await _data.ToggleFavoriteAsync(r.Id);
        await LoadAsync();
    }
}

// === FILTERS (modal) ===

public partial class FiltersViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public List<FilterChip> Categories { get; } = new()
    {
        new("Popular"), new("Trending"), new("Recent"),
    };
    public List<FilterChip> Times { get; } = new()
    {
        new("15 min"), new("30 min"), new("60 min"),
    };
    public List<FilterChip> Skills { get; } = new()
    {
        new("Beginner"), new("Intermediate"), new("Advanced"),
    };

    public FiltersViewModel(INavigator navigator) { _navigator = navigator; }

    [RelayCommand]
    private void ToggleChip(FilterChip? chip) { if (chip is not null) chip.IsSelected = !chip.IsSelected; }

    [RelayCommand]
    private void Reset()
    {
        foreach (var c in Categories) c.IsSelected = false;
        foreach (var c in Times) c.IsSelected = false;
        foreach (var c in Skills) c.IsSelected = false;
    }

    [RelayCommand]
    private async Task ApplyAsync() => await _navigator.NavigateRouteAsync(this, "MainShell");

    [RelayCommand]
    private async Task CloseAsync() => await _navigator.NavigateRouteAsync(this, "MainShell");
}

public partial class FilterChip : ObservableObject
{
    public FilterChip(string label) { Label = label; }
    public string Label { get; }
    [ObservableProperty] private bool isSelected;
}

// === RECIPE DETAIL ===

public partial class RecipeDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private RecipeData? recipe;
    [ObservableProperty] private int selectedTabIndex;
    [ObservableProperty] private NutritionData nutrition = new() { Protein = 30, ProteinBase = 110, Carbs = 101, CarbsBase = 300, Fat = 30, FatBase = 75 };

    public bool HasReviews => Recipe?.Reviews is { Count: > 0 };
    public bool HasNoReviews => !HasReviews;
    public int ReviewCount => Recipe?.Reviews?.Count ?? 0;
    public int IngredientCount => Recipe?.Ingredients?.Count ?? 0;
    public int StepCount => Recipe?.Steps?.Count ?? 0;
    public string AuthorName => Recipe?.Creator?.FullName ?? "Niki Samantha";
    public string AuthorAvatar => Recipe?.Creator?.UrlProfileImage ?? "ms-appx:///Assets/Profiles/niki_samantha.png";

    public double ProteinPercent => Nutrition.ProteinBase > 0 ? Math.Min(100, Nutrition.Protein / Nutrition.ProteinBase * 100) : 0;
    public double CarbsPercent => Nutrition.CarbsBase > 0 ? Math.Min(100, Nutrition.Carbs / Nutrition.CarbsBase * 100) : 0;
    public double FatPercent => Nutrition.FatBase > 0 ? Math.Min(100, Nutrition.Fat / Nutrition.FatBase * 100) : 0;

    public RecipeDetailViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await _data.InitializeAsync();
        var id = _data.SelectedRecipeId;
        Recipe = id.HasValue ? await _data.GetRecipeAsync(id.Value)
                              : (await _data.GetAllRecipesAsync()).FirstOrDefault();
        // Use derived nutrition from recipe if present
        if (Recipe?.Nutrition is { } n) Nutrition = n;
        OnPropertyChanged(nameof(HasReviews));
        OnPropertyChanged(nameof(HasNoReviews));
        OnPropertyChanged(nameof(ReviewCount));
        OnPropertyChanged(nameof(IngredientCount));
        OnPropertyChanged(nameof(StepCount));
        OnPropertyChanged(nameof(AuthorName));
        OnPropertyChanged(nameof(AuthorAvatar));
        OnPropertyChanged(nameof(ProteinPercent));
        OnPropertyChanged(nameof(CarbsPercent));
        OnPropertyChanged(nameof(FatPercent));
    }

    [RelayCommand]
    private async Task StartCookingAsync()
    {
        if (Recipe is null) return;
        _data.SelectedRecipeId = Recipe.Id;
        await _navigator.NavigateRouteAsync(this, "/LiveCooking");
    }

    [RelayCommand]
    private async Task BackAsync() => await _navigator.NavigateRouteAsync(this, "MainShell");

    [RelayCommand]
    private async Task ToggleFavoriteAsync()
    {
        if (Recipe is null) return;
        await _data.ToggleFavoriteAsync(Recipe.Id);
        Recipe = Recipe with { IsFavorite = !Recipe.IsFavorite };
    }
}

// === LIVE COOKING ===

public partial class LiveCookingViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private RecipeData? recipe;
    [ObservableProperty] private int currentStepIndex;
    public bool HasPrevious => CurrentStepIndex > 0;
    public bool HasNext => Recipe?.Steps is { } steps && CurrentStepIndex < steps.Count - 1;
    public StepData? CurrentStep => Recipe?.Steps?.ElementAtOrDefault(CurrentStepIndex);
    public int TotalSteps => Recipe?.Steps?.Count ?? 0;
    public string Title => Recipe is null ? "Live Cooking" : $"Making {Recipe.Name}";

    partial void OnCurrentStepIndexChanged(int value)
    {
        OnPropertyChanged(nameof(HasPrevious));
        OnPropertyChanged(nameof(HasNext));
        OnPropertyChanged(nameof(CurrentStep));
    }

    public LiveCookingViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await _data.InitializeAsync();
        var id = _data.SelectedRecipeId;
        Recipe = id.HasValue ? await _data.GetRecipeAsync(id.Value)
                              : (await _data.GetAllRecipesAsync()).FirstOrDefault();
        CurrentStepIndex = 0;
        OnPropertyChanged(nameof(CurrentStep));
        OnPropertyChanged(nameof(TotalSteps));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(HasPrevious));
        OnPropertyChanged(nameof(HasNext));
    }

    [RelayCommand]
    private async Task NextAsync()
    {
        if (Recipe?.Steps is null) return;
        if (CurrentStepIndex < Recipe.Steps.Count - 1) CurrentStepIndex++;
        else
        {
            _data.SelectedRecipeId = Recipe.Id;
            await _navigator.NavigateRouteAsync(this, "/LiveCookingFinish");
        }
    }

    [RelayCommand] private void Previous() { if (CurrentStepIndex > 0) CurrentStepIndex--; }

    [RelayCommand]
    private async Task BackAsync() => await _navigator.NavigateRouteAsync(this, "MainShell");
}

// === LIVE COOKING FINISH ===

public partial class LiveCookingFinishViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private RecipeData? recipe;
    [ObservableProperty] private int rating;
    public bool IsFavorite => Recipe?.IsFavorite ?? false;
    public string Title => Recipe is null ? "Live Cooking" : $"Making {Recipe.Name}";

    public LiveCookingFinishViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await _data.InitializeAsync();
        var id = _data.SelectedRecipeId;
        if (id.HasValue) Recipe = await _data.GetRecipeAsync(id.Value);
        OnPropertyChanged(nameof(IsFavorite));
        OnPropertyChanged(nameof(Title));
    }

    [RelayCommand] private void RateOne() => Rating = 1;
    [RelayCommand] private void RateTwo() => Rating = 2;
    [RelayCommand] private void RateThree() => Rating = 3;
    [RelayCommand] private void RateFour() => Rating = 4;
    [RelayCommand] private void RateFive() => Rating = 5;

    [RelayCommand]
    private async Task FavoriteAsync()
    {
        if (Recipe is null) return;
        await _data.ToggleFavoriteAsync(Recipe.Id);
        Recipe = Recipe with { IsFavorite = !Recipe.IsFavorite };
        OnPropertyChanged(nameof(IsFavorite));
    }

    [RelayCommand]
    private async Task PreviousAsync() => await _navigator.NavigateRouteAsync(this, "MainShell");

    [RelayCommand]
    private async Task BackAsync() => await _navigator.NavigateRouteAsync(this, "MainShell");
}

// === FAVORITES (Recipes + Cookbooks shared) ===

public partial class FavoritesRecipesViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private ObservableCollection<RecipeData> items = new();
    public bool HasItems => Items.Count > 0;
    public bool IsEmpty => Items.Count == 0;

    public FavoritesRecipesViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        _data.FavoritesChanged += async (_, _) => await LoadAsync();
        Items.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(IsEmpty));
        };
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await _data.InitializeAsync();
        var favs = await _data.GetFavoritedAsync();
        Items.Clear();
        foreach (var r in favs) Items.Add(r);
    }

    [RelayCommand]
    private async Task OpenRecipeAsync(RecipeData? r)
    {
        if (r is null) return;
        _data.SelectedRecipeId = r.Id;
        await _navigator.NavigateRouteAsync(this, "RecipeDetail");
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync(RecipeData? r)
    {
        if (r is null) return;
        await _data.ToggleFavoriteAsync(r.Id);
    }

    [RelayCommand]
    private async Task SeePopularAsync() => await _navigator.NavigateRouteAsync(this, "/MainShell");
}

public partial class FavoritesCookbooksViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private ObservableCollection<CookbookData> items = new();
    public bool HasItems => Items.Count > 0;
    public bool IsEmpty => Items.Count == 0;

    public FavoritesCookbooksViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        _data.CookbooksChanged += async (_, _) => await LoadAsync();
        Items.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(IsEmpty));
        };
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await _data.InitializeAsync();
        var saved = await _data.GetSavedCookbooksAsync();
        Items.Clear();
        foreach (var c in saved) Items.Add(c);
    }

    [RelayCommand]
    private async Task OpenCookbookAsync(CookbookData? c)
    {
        if (c is null) return;
        _data.SelectedCookbookId = c.Id;
        await _navigator.NavigateRouteAsync(this, "/CookbookDetail");
    }

    [RelayCommand]
    private async Task CreateCookbookAsync() => await _navigator.NavigateRouteAsync(this, "/CreateCookbook");
}

public partial class FavoritesViewModel : ObservableObject
{
    [ObservableProperty] private int selectedTab;
    public FavoritesRecipesViewModel Recipes { get; }
    public FavoritesCookbooksViewModel Cookbooks { get; }

    public FavoritesViewModel(FavoritesRecipesViewModel recipes, FavoritesCookbooksViewModel cookbooks)
    {
        Recipes = recipes;
        Cookbooks = cookbooks;
    }
}

// === COOKBOOK DETAIL ===

public partial class CookbookDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private CookbookData? cookbook;
    [ObservableProperty] private ObservableCollection<RecipeData> recipes = new();
    public int RecipeCount => Recipes.Count;
    public string CookbookName => Cookbook?.Name ?? "Cookbook";

    public CookbookDetailViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        Recipes.CollectionChanged += (_, _) => OnPropertyChanged(nameof(RecipeCount));
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await _data.InitializeAsync();
        var id = _data.SelectedCookbookId;
        var all = await _data.GetCookbooksAsync();
        Cookbook = id.HasValue ? all.FirstOrDefault(c => c.Id == id.Value) : all.FirstOrDefault();
        OnPropertyChanged(nameof(CookbookName));
        Recipes.Clear();
        if (Cookbook?.Recipes is { } list) foreach (var r in list) Recipes.Add(r);
    }

    [RelayCommand]
    private async Task OpenRecipeAsync(RecipeData? r)
    {
        if (r is null) return;
        _data.SelectedRecipeId = r.Id;
        await _navigator.NavigateRouteAsync(this, "RecipeDetail");
    }

    [RelayCommand]
    private async Task UpdateAsync()
    {
        if (Cookbook is null) return;
        _data.SelectedCookbookId = Cookbook.Id;
        await _navigator.NavigateRouteAsync(this, "/UpdateCookbook");
    }

    [RelayCommand]
    private async Task BackAsync() => await _navigator.NavigateRouteAsync(this, "MainShell");
}

// === CREATE / UPDATE COOKBOOK ===

public partial class CreateCookbookViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private ObservableCollection<RecipePickItem> picks = new();

    public CreateCookbookViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await _data.InitializeAsync();
        var all = await _data.GetAllRecipesAsync();
        Picks.Clear();
        foreach (var r in all.Take(20)) Picks.Add(new RecipePickItem(r, false));
    }

    [RelayCommand]
    private void TogglePick(RecipePickItem? item) { if (item is not null) item.Selected = !item.Selected; }

    [RelayCommand]
    private async Task CreateAsync()
    {
        if (string.IsNullOrWhiteSpace(Name)) Name = "New Cookbook";
        var picks = Picks.Where(p => p.Selected).Select(p => p.Recipe).ToList();
        var cookbook = new CookbookData
        {
            Id = Guid.NewGuid(),
            UserId = _data.CurrentUser?.Id ?? Guid.Empty,
            Name = Name,
            Recipes = picks,
            PinsNumber = picks.Count,
        };
        await _data.SaveCookbookAsync(cookbook);
        await _navigator.NavigateRouteAsync(this, "MainShell");
    }

    [RelayCommand]
    private async Task CancelAsync() => await _navigator.NavigateRouteAsync(this, "MainShell");
}

public partial class UpdateCookbookViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private CookbookData? cookbook;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private ObservableCollection<RecipePickItem> picks = new();

    public UpdateCookbookViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await _data.InitializeAsync();
        var id = _data.SelectedCookbookId;
        var all = await _data.GetCookbooksAsync();
        Cookbook = id.HasValue ? all.FirstOrDefault(c => c.Id == id.Value) : all.FirstOrDefault();
        Name = Cookbook?.Name ?? "";
        var allRecipes = await _data.GetAllRecipesAsync();
        var existingIds = new HashSet<Guid>(Cookbook?.Recipes?.Select(r => r.Id) ?? Array.Empty<Guid>());
        Picks.Clear();
        foreach (var r in allRecipes.Take(20)) Picks.Add(new RecipePickItem(r, existingIds.Contains(r.Id)));
    }

    [RelayCommand]
    private void TogglePick(RecipePickItem? item) { if (item is not null) item.Selected = !item.Selected; }

    [RelayCommand]
    private async Task ApplyAsync()
    {
        if (Cookbook is null) return;
        var picks = Picks.Where(p => p.Selected).Select(p => p.Recipe).ToList();
        var updated = Cookbook with { Name = Name, Recipes = picks, PinsNumber = picks.Count };
        await _data.SaveCookbookAsync(updated);
        await _navigator.NavigateRouteAsync(this, "MainShell");
    }

    [RelayCommand]
    private async Task CancelAsync() => await _navigator.NavigateRouteAsync(this, "MainShell");
}

public partial class RecipePickItem : ObservableObject
{
    public RecipePickItem(RecipeData recipe, bool selected) { Recipe = recipe; Selected = selected; }
    public RecipeData Recipe { get; }
    [ObservableProperty] private bool selected;
}

// === PROFILE ===

public partial class ProfileViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private UserData? user;
    [ObservableProperty] private ObservableCollection<RecipeData> myRecipes = new();
    public bool HasRecipes => MyRecipes.Count > 0;
    public bool IsEmpty => MyRecipes.Count == 0;

    public ProfileViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        MyRecipes.CollectionChanged += (_, _) => { OnPropertyChanged(nameof(HasRecipes)); OnPropertyChanged(nameof(IsEmpty)); };
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await _data.InitializeAsync();
        User = _data.CurrentUser ?? (await _data.GetUsersAsync()).FirstOrDefault();
        var allRecipes = await _data.GetAllRecipesAsync();
        var mine = allRecipes.Where(r => User is not null && r.UserId == User.Id).ToList();
        if (mine.Count == 0) mine = allRecipes.Take(4).ToList();
        MyRecipes.Clear();
        foreach (var r in mine) MyRecipes.Add(r);
    }

    [RelayCommand]
    private async Task SettingsAsync() => await _navigator.NavigateRouteAsync(this, "/Settings");

    [RelayCommand]
    private async Task NotificationsAsync() => await _navigator.NavigateRouteAsync(this, "/Notifications");

    [RelayCommand]
    private async Task BackAsync() => await _navigator.NavigateRouteAsync(this, "MainShell");

    [RelayCommand]
    private async Task OpenRecipeAsync(RecipeData? r)
    {
        if (r is null) return;
        _data.SelectedRecipeId = r.Id;
        await _navigator.NavigateRouteAsync(this, "RecipeDetail");
    }
}

public partial class OtherProfileViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private UserData? user;
    [ObservableProperty] private ObservableCollection<RecipeData> recipes = new();
    public bool HasRecipes => Recipes.Count > 0;

    public OtherProfileViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        Recipes.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasRecipes));
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await _data.InitializeAsync();
        var id = _data.SelectedUserId;
        var all = await _data.GetUsersAsync();
        User = id.HasValue ? all.FirstOrDefault(u => u.Id == id.Value) : all.FirstOrDefault();
        var allRecipes = await _data.GetAllRecipesAsync();
        Recipes.Clear();
        foreach (var r in allRecipes.Where(r => User is not null && r.UserId == User.Id).Take(8)) Recipes.Add(r);
        // If selected user has no authored recipes in fixtures, show some popular ones
        if (Recipes.Count == 0)
            foreach (var r in allRecipes.Take(4)) Recipes.Add(r);
    }

    [RelayCommand]
    private async Task BackAsync() => await _navigator.NavigateRouteAsync(this, "MainShell");

    [RelayCommand]
    private async Task OpenRecipeAsync(RecipeData? r)
    {
        if (r is null) return;
        _data.SelectedRecipeId = r.Id;
        await _navigator.NavigateRouteAsync(this, "RecipeDetail");
    }
}

// === SETTINGS ===

public partial class SettingsViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string emailAddress = string.Empty;
    [ObservableProperty] private string phoneNumber = string.Empty;
    [ObservableProperty] private bool notificationsEnabled = true;
    [ObservableProperty] private bool nightModeEnabled;
    [ObservableProperty] private string? statusMessage;

    public SettingsViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        var u = _data.CurrentUser;
        if (u is not null)
        {
            FullName = u.FullName ?? "";
            EmailAddress = u.Email ?? "";
            PhoneNumber = u.PhoneNumber ?? "";
        }
        NotificationsEnabled = _data.NotificationsEnabled;
        NightModeEnabled = _data.NightModeEnabled;
    }

    partial void OnNightModeEnabledChanged(bool value) => _data.NightModeEnabled = value;
    partial void OnNotificationsEnabledChanged(bool value) => _data.NotificationsEnabled = value;

    [RelayCommand]
    private void Save()
    {
        StatusMessage = "Saved at " + DateTime.Now.ToShortTimeString();
    }

    [RelayCommand]
    private async Task LogOutAsync() => await _navigator.NavigateRouteAsync(this, "/Login");

    [RelayCommand]
    private async Task BackAsync() => await _navigator.NavigateRouteAsync(this, "MainShell");
}

// === NOTIFICATIONS ===

public partial class NotificationsViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;
    private IReadOnlyList<NotificationData> _all = Array.Empty<NotificationData>();

    [ObservableProperty] private int selectedTab; // 0 = All, 1 = Unread, 2 = Read
    [ObservableProperty] private ObservableCollection<NotificationData> visible = new();
    public bool HasItems => Visible.Count > 0;
    public bool IsEmpty => Visible.Count == 0;

    public NotificationsViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        Visible.CollectionChanged += (_, _) => { OnPropertyChanged(nameof(HasItems)); OnPropertyChanged(nameof(IsEmpty)); };
        _ = LoadAsync();
    }


    private async Task LoadAsync()
    {
        await _data.InitializeAsync();
        _all = await _data.GetNotificationsAsync();
        ApplyFilter();
    }

    partial void OnSelectedTabChanged(int value) => ApplyFilter();

    private void ApplyFilter()
    {
        Visible.Clear();
        IEnumerable<NotificationData> source = SelectedTab switch
        {
            1 => _all.Where(n => !n.IsRead),
            2 => _all.Where(n => n.IsRead),
            _ => _all,
        };
        foreach (var n in source.OrderByDescending(n => n.Date)) Visible.Add(n);
    }

    [RelayCommand]
    private async Task CloseAsync() => await _navigator.NavigateRouteAsync(this, "MainShell");
}

// === NEAR ME MAP ===

public partial class NearMeViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private ObservableCollection<UserData> contributors = new();
    [ObservableProperty] private UserData? selectedContributor;

    public NearMeViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await _data.InitializeAsync();
        var creators = await _data.GetPopularCreatorsAsync();
        Contributors.Clear();
        foreach (var u in creators.Take(5)) Contributors.Add(u);
        SelectedContributor = Contributors.FirstOrDefault();
    }

    [RelayCommand]
    private void SelectContributor(UserData? user) { if (user is not null) SelectedContributor = user; }

    [RelayCommand]
    private async Task BackAsync() => await _navigator.NavigateRouteAsync(this, "MainShell");
}
