using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ChefsTest7.Models;
using ChefsTest7.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest7.Presentation.Pages;

public abstract partial class ChefsViewModelBase : ObservableObject
{
    protected readonly INavigator Navigator;
    protected readonly IChefsDataService Data;

    protected ChefsViewModelBase(INavigator navigator, IChefsDataService data)
    {
        Navigator = navigator;
        Data = data;
    }

    [RelayCommand]
    protected async Task GoBack() => await Navigator.NavigateBackAsync(this);
}

public partial class OnboardingViewModel : ChefsViewModelBase
{
    public OnboardingViewModel(INavigator navigator, IChefsDataService data) : base(navigator, data) { }

    [RelayCommand]
    private async Task Skip() => await Navigator.NavigateRouteAsync(this, "-/Login");

    [RelayCommand]
    private async Task GetStarted() => await Navigator.NavigateRouteAsync(this, "-/Login");
}

public partial class LoginViewModel : ChefsViewModelBase
{
    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private bool rememberMe;

    public LoginViewModel(INavigator navigator, IChefsDataService data) : base(navigator, data) { }

    [RelayCommand]
    private async Task Login()
    {
        await Data.InitializeAsync();
        if (Data.Users.Count > 0)
        {
            var user = Data.Users.FirstOrDefault(u => string.Equals(u.FullName, Username, StringComparison.OrdinalIgnoreCase))
                       ?? Data.Users.First();
            Data.SetCurrentUser(user);
        }
        await Navigator.NavigateRouteAsync(this, "-/Main");
    }

    [RelayCommand]
    private async Task GoToRegister() => await Navigator.NavigateRouteAsync(this, "Register");

    [RelayCommand] private Task SignInWithApple() => Login();
    [RelayCommand] private Task SignInWithGoogle() => Login();
    [RelayCommand] private Task ForgotPassword() => Task.CompletedTask;
}

public partial class RegisterViewModel : ChefsViewModelBase
{
    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;

    public RegisterViewModel(INavigator navigator, IChefsDataService data) : base(navigator, data) { }

    [RelayCommand]
    private async Task SignUp() => await Navigator.NavigateRouteAsync(this, "-/Main");

    [RelayCommand]
    private async Task BackToLogin() => await Navigator.NavigateBackAsync(this);
}

public partial class MainShellViewModel : ChefsViewModelBase
{
    public MainShellViewModel(INavigator navigator, IChefsDataService data) : base(navigator, data)
    {
        _ = Data.InitializeAsync();
    }
}

public partial class HomeViewModel : ChefsViewModelBase
{
    public ObservableCollection<Recipe> TrendingRecipes { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<Recipe> RecentlyAdded { get; } = new();
    public ObservableCollection<User> PopularContributors { get; } = new();

    public HomeViewModel(INavigator navigator, IChefsDataService data) : base(navigator, data)
    {
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await Data.InitializeAsync();
        TrendingRecipes.Clear();
        foreach (var r in Data.Recipes.Take(8)) TrendingRecipes.Add(r);
        Categories.Clear();
        foreach (var c in Data.Categories) Categories.Add(c);
        RecentlyAdded.Clear();
        foreach (var r in Data.Recipes.Skip(2).Take(8)) RecentlyAdded.Add(r);
        PopularContributors.Clear();
        foreach (var u in Data.Users.Take(8)) PopularContributors.Add(u);
        OnPropertyChanged(nameof(TrendingRecipes));
    }

    [RelayCommand]
    private async Task OpenRecipe(Recipe? recipe)
    {
        if (recipe is null) return;
        await Navigator.NavigateDataAsync(this, recipe);
    }

    [RelayCommand]
    private async Task OpenContributor(User? user)
    {
        if (user is null) return;
        await Navigator.NavigateDataAsync(this, user);
    }

    [RelayCommand]
    private async Task OpenNearMe() => await Navigator.NavigateRouteAsync(this, "NearMeMap");

    [RelayCommand]
    private async Task OpenNotifications() => await Navigator.NavigateRouteAsync(this, "Notifications");

    [RelayCommand]
    private async Task OpenProfile() => await Navigator.NavigateRouteAsync(this, "Profile");
}

public partial class SearchViewModel : ChefsViewModelBase
{
    [ObservableProperty] private string query = string.Empty;
    public ObservableCollection<Recipe> Results { get; } = new();

    public int ResultCount => Results.Count;
    public bool HasResults => Results.Count > 0;
    public bool IsEmpty => !HasResults && !string.IsNullOrEmpty(Query);

    public SearchViewModel(INavigator navigator, IChefsDataService data) : base(navigator, data)
    {
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await Data.InitializeAsync();
        ApplyQuery(string.Empty);
    }

    partial void OnQueryChanged(string value) => ApplyQuery(value);

    private void ApplyQuery(string q)
    {
        Results.Clear();
        var src = string.IsNullOrWhiteSpace(q)
            ? Data.Recipes
            : Data.Recipes.Where(r => r.Name.Contains(q, StringComparison.OrdinalIgnoreCase));
        foreach (var r in src) Results.Add(r);
        OnPropertyChanged(nameof(ResultCount));
        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(IsEmpty));
    }

    [RelayCommand]
    private async Task OpenFilters() => await Navigator.NavigateRouteAsync(this, "!Filters");

    [RelayCommand]
    private async Task OpenRecipe(Recipe? recipe)
    {
        if (recipe is null) return;
        await Navigator.NavigateDataAsync(this, recipe);
    }
}

public partial class FiltersViewModel : ChefsViewModelBase
{
    public ObservableCollection<Category> AllCategories { get; } = new();
    [ObservableProperty] private int? cookingTimeIndex;
    [ObservableProperty] private int? skillLevelIndex;

    public FiltersViewModel(INavigator navigator, IChefsDataService data) : base(navigator, data)
    {
        foreach (var c in data.Categories) AllCategories.Add(c);
    }

    [RelayCommand] private async Task Apply() => await Navigator.NavigateBackAsync(this);
    [RelayCommand] private void Reset() { CookingTimeIndex = null; SkillLevelIndex = null; }
    [RelayCommand] private async Task Close() => await Navigator.NavigateBackAsync(this);
}

public partial class RecipeDetailViewModel : ChefsViewModelBase
{
    [ObservableProperty] private Recipe? recipe;
    [ObservableProperty] private int selectedTabIndex;
    [ObservableProperty] private bool isFavorite;

    public User? Author => Recipe is null ? null : Data.Users.FirstOrDefault(u => u.Id == Recipe.UserId);
    public IList<RecipeStep> Steps => Recipe?.Steps ?? new List<RecipeStep>();
    public IList<Ingredient> Ingredients => Recipe?.Ingredients ?? new List<Ingredient>();
    public IList<Review> Reviews => Recipe?.Reviews ?? new List<Review>();
    public bool HasReviews => Reviews.Count > 0;

    public RecipeDetailViewModel(Recipe recipe, INavigator navigator, IChefsDataService data) : base(navigator, data)
    {
        Recipe = recipe;
        IsFavorite = data.SavedRecipeIds.Contains(recipe.Id);
    }

    [RelayCommand]
    private void ToggleFavorite()
    {
        if (Recipe is null) return;
        IsFavorite = Data.ToggleSavedRecipe(Recipe.Id);
    }

    [RelayCommand]
    private async Task StartCooking()
    {
        if (Recipe is null) return;
        await Navigator.NavigateDataAsync(this, Recipe);
    }
}

public partial class LiveCookingViewModel : ChefsViewModelBase
{
    [ObservableProperty] private Recipe? recipe;
    [ObservableProperty] private int currentStepIndex;

    public RecipeStep? CurrentStep => Recipe?.Steps?.Count > CurrentStepIndex && CurrentStepIndex >= 0 ? Recipe.Steps[CurrentStepIndex] : null;
    public bool CanGoPrevious => CurrentStepIndex > 0;
    public bool CanGoNext => Recipe?.Steps?.Count > CurrentStepIndex + 1;

    public LiveCookingViewModel(Recipe recipe, INavigator navigator, IChefsDataService data) : base(navigator, data)
    {
        Recipe = recipe;
    }

    [RelayCommand]
    private void Previous()
    {
        if (CanGoPrevious) { CurrentStepIndex--; OnPropertyChanged(nameof(CurrentStep)); }
    }

    [RelayCommand]
    private async Task NextStep()
    {
        if (CanGoNext) { CurrentStepIndex++; OnPropertyChanged(nameof(CurrentStep)); }
        else { await Navigator.NavigateRouteAsync(this, "LiveCookingFinish"); }
    }

    partial void OnCurrentStepIndexChanged(int value)
    {
        OnPropertyChanged(nameof(CurrentStep));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
    }
}

public partial class LiveCookingFinishViewModel : ChefsViewModelBase
{
    [ObservableProperty] private int rating = 5;

    public LiveCookingFinishViewModel(INavigator navigator, IChefsDataService data) : base(navigator, data) { }

    [RelayCommand]
    private async Task Done() => await Navigator.NavigateRouteAsync(this, "-/Main");
}

public partial class FavoritesAllViewModel : ChefsViewModelBase
{
    public ObservableCollection<Recipe> SavedRecipes { get; } = new();
    public bool IsEmpty => SavedRecipes.Count == 0;

    public FavoritesAllViewModel(INavigator navigator, IChefsDataService data) : base(navigator, data)
    {
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await Data.InitializeAsync();
        SavedRecipes.Clear();
        foreach (var r in Data.Recipes.Where(r => Data.SavedRecipeIds.Contains(r.Id)))
            SavedRecipes.Add(r);
        if (SavedRecipes.Count == 0)
        {
            foreach (var r in Data.Recipes.Take(6)) SavedRecipes.Add(r);
        }
        OnPropertyChanged(nameof(IsEmpty));
    }

    [RelayCommand]
    private async Task OpenRecipe(Recipe? recipe)
    {
        if (recipe is null) return;
        await Navigator.NavigateDataAsync(this, recipe);
    }

    [RelayCommand] private async Task GoToCookbooks() => await Navigator.NavigateRouteAsync(this, "FavoritesCookbooks");
}

public partial class FavoritesCookbooksViewModel : ChefsViewModelBase
{
    public ObservableCollection<Cookbook> SavedCookbooks { get; } = new();
    public bool IsEmpty => SavedCookbooks.Count == 0;

    public FavoritesCookbooksViewModel(INavigator navigator, IChefsDataService data) : base(navigator, data)
    {
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await Data.InitializeAsync();
        SavedCookbooks.Clear();
        foreach (var c in Data.Cookbooks) SavedCookbooks.Add(c);
        OnPropertyChanged(nameof(IsEmpty));
    }

    [RelayCommand]
    private async Task OpenCookbook(Cookbook? cookbook)
    {
        if (cookbook is null) return;
        await Navigator.NavigateDataAsync(this, cookbook);
    }

    [RelayCommand]
    private async Task CreateCookbook() => await Navigator.NavigateRouteAsync(this, "CreateCookbook");

    [RelayCommand] private async Task GoToAll() => await Navigator.NavigateRouteAsync(this, "FavoritesAll");
}

public partial class CookbookDetailViewModel : ChefsViewModelBase
{
    [ObservableProperty] private Cookbook? cookbook;
    public IList<Recipe> Recipes => Cookbook?.Recipes ?? new List<Recipe>();
    public int RecipeCount => Recipes.Count;

    public CookbookDetailViewModel(Cookbook cookbook, INavigator navigator, IChefsDataService data) : base(navigator, data)
    {
        Cookbook = cookbook;
    }

    [RelayCommand]
    private async Task OpenRecipe(Recipe? recipe)
    {
        if (recipe is null) return;
        await Navigator.NavigateDataAsync(this, recipe);
    }

    [RelayCommand]
    private async Task EditCookbook()
    {
        if (Cookbook is null) return;
        await Navigator.NavigateDataAsync(this, Cookbook);
    }
}

public partial class CreateCookbookViewModel : ChefsViewModelBase
{
    [ObservableProperty] private string name = string.Empty;
    public ObservableCollection<RecipePickItem> AllRecipes { get; } = new();

    public CreateCookbookViewModel(INavigator navigator, IChefsDataService data) : base(navigator, data)
    {
        foreach (var r in data.Recipes) AllRecipes.Add(new RecipePickItem(r));
    }

    [RelayCommand]
    private async Task Create()
    {
        var picks = AllRecipes.Where(p => p.IsPicked).Select(p => p.Recipe).ToList();
        var cb = new Cookbook(Guid.NewGuid().ToString(), Name, Data.CurrentUser?.Id ?? string.Empty, picks.Count, picks);
        Data.AddCookbook(cb);
        await Navigator.NavigateBackAsync(this);
    }

    [RelayCommand]
    private async Task Cancel() => await Navigator.NavigateBackAsync(this);
}

public partial class RecipePickItem : ObservableObject
{
    public Recipe Recipe { get; }
    [ObservableProperty] private bool isPicked;
    public RecipePickItem(Recipe r) { Recipe = r; }
}

public partial class UpdateCookbookViewModel : ChefsViewModelBase
{
    [ObservableProperty] private Cookbook? cookbook;
    [ObservableProperty] private string name = string.Empty;
    public ObservableCollection<RecipePickItem> AllRecipes { get; } = new();

    public UpdateCookbookViewModel(Cookbook cookbook, INavigator navigator, IChefsDataService data) : base(navigator, data)
    {
        Cookbook = cookbook;
        Name = cookbook.Name;
        var pickedIds = (cookbook.Recipes ?? new List<Recipe>()).Select(r => r.Id).ToHashSet();
        foreach (var r in data.Recipes)
            AllRecipes.Add(new RecipePickItem(r) { IsPicked = pickedIds.Contains(r.Id) });
    }

    [RelayCommand]
    private async Task Apply()
    {
        if (Cookbook is null) return;
        var picks = AllRecipes.Where(p => p.IsPicked).Select(p => p.Recipe).ToList();
        var updated = Cookbook with { Name = Name, PinsNumber = picks.Count, Recipes = picks };
        Data.UpdateCookbook(updated);
        await Navigator.NavigateBackAsync(this);
    }

    [RelayCommand]
    private async Task Cancel() => await Navigator.NavigateBackAsync(this);
}

public partial class ProfileViewModel : ChefsViewModelBase
{
    public User? CurrentUser => Data.CurrentUser;
    public ObservableCollection<Recipe> MyRecipes { get; } = new();
    public bool IsEmpty => MyRecipes.Count == 0;

    public ProfileViewModel(INavigator navigator, IChefsDataService data) : base(navigator, data)
    {
        if (data.CurrentUser is { } u)
            foreach (var r in data.Recipes.Where(r => r.UserId == u.Id))
                MyRecipes.Add(r);
        if (MyRecipes.Count == 0)
            foreach (var r in data.Recipes.Take(4)) MyRecipes.Add(r);
    }

    [RelayCommand]
    private async Task OpenRecipe(Recipe? recipe)
    {
        if (recipe is null) return;
        await Navigator.NavigateDataAsync(this, recipe);
    }

    [RelayCommand] private async Task OpenSettings() => await Navigator.NavigateRouteAsync(this, "Settings");
    [RelayCommand] private async Task AddRecipe() => await Navigator.NavigateRouteAsync(this, "CreateCookbook");
}

public partial class OtherProfileViewModel : ChefsViewModelBase
{
    [ObservableProperty] private User? user;
    public ObservableCollection<Recipe> Recipes { get; } = new();

    public OtherProfileViewModel(User user, INavigator navigator, IChefsDataService data) : base(navigator, data)
    {
        User = user;
        foreach (var r in data.Recipes.Where(r => r.UserId == user.Id)) Recipes.Add(r);
        if (Recipes.Count == 0)
            foreach (var r in data.Recipes.Take(4)) Recipes.Add(r);
    }

    [RelayCommand]
    private async Task OpenRecipe(Recipe? recipe)
    {
        if (recipe is null) return;
        await Navigator.NavigateDataAsync(this, recipe);
    }
}

public partial class SettingsViewModel : ChefsViewModelBase
{
    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string mobileNumber = string.Empty;
    [ObservableProperty] private bool notificationsEnabled = true;
    [ObservableProperty] private bool nightModeEnabled;

    public SettingsViewModel(INavigator navigator, IChefsDataService data) : base(navigator, data)
    {
        if (data.CurrentUser is { } u)
        {
            FullName = u.FullName;
            Email = u.Email;
            MobileNumber = u.PhoneNumber;
        }
        var dispatcher = Microsoft.UI.Xaml.Application.Current?.RequestedTheme;
        NightModeEnabled = dispatcher == Microsoft.UI.Xaml.ApplicationTheme.Dark;
    }

    [RelayCommand]
    private async Task SaveChanges() => await Navigator.NavigateBackAsync(this);

    [RelayCommand]
    private async Task LogOut() => await Navigator.NavigateRouteAsync(this, "-/Login");

    partial void OnNightModeEnabledChanged(bool value)
    {
        if (Microsoft.UI.Xaml.Window.Current?.Content is FrameworkElement root)
        {
            root.RequestedTheme = value ? ElementTheme.Dark : ElementTheme.Light;
        }
    }
}

public partial class NotificationsViewModel : ChefsViewModelBase
{
    public ObservableCollection<Notification> All { get; } = new();
    public ObservableCollection<Notification> Unread { get; } = new();
    public ObservableCollection<Notification> Read { get; } = new();
    [ObservableProperty] private int selectedSegment;

    public bool IsEmpty => All.Count == 0;

    public NotificationsViewModel(INavigator navigator, IChefsDataService data) : base(navigator, data)
    {
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await Data.InitializeAsync();
        foreach (var n in Data.Notifications) All.Add(n);
        foreach (var n in Data.Notifications.Where(n => !n.IsRead)) Unread.Add(n);
        foreach (var n in Data.Notifications.Where(n => n.IsRead)) Read.Add(n);
        OnPropertyChanged(nameof(IsEmpty));
    }

    [RelayCommand]
    private async Task Close() => await Navigator.NavigateBackAsync(this);
}

public partial class NearMeMapViewModel : ChefsViewModelBase
{
    public ObservableCollection<User> Chefs { get; } = new();
    [ObservableProperty] private User? selectedChef;

    public NearMeMapViewModel(INavigator navigator, IChefsDataService data) : base(navigator, data)
    {
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await Data.InitializeAsync();
        foreach (var u in Data.Users) Chefs.Add(u);
        SelectedChef = Chefs.FirstOrDefault();
    }

    [RelayCommand]
    private async Task OpenChef(User? user)
    {
        if (user is null) return;
        await Navigator.NavigateDataAsync(this, user);
    }
}

