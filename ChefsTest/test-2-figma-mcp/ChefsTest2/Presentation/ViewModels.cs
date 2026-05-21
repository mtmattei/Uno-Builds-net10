using System.Collections.ObjectModel;
using ChefsTest2.Models;
using ChefsTest2.Services;

namespace ChefsTest2.Presentation;

public abstract partial class ChefsViewModelBase : ObservableObject
{
    protected readonly INavigator Navigator;
    protected readonly IDataService Data;

    protected ChefsViewModelBase(INavigator navigator, IDataService data)
    {
        Navigator = navigator;
        Data = data;
    }

    [RelayCommand]
    private async Task GoBack() => await Navigator.NavigateBackAsync(this);
}

public partial class OnboardingViewModel : ChefsViewModelBase
{
    public OnboardingViewModel(INavigator navigator, IDataService data) : base(navigator, data) { }

    [ObservableProperty] private int currentPage;

    public string[] Titles { get; } = new[]
    {
        "Welcome to Your App!",
        "Discover Tasty Recipes",
        "Cook Like a Pro Chef",
    };

    public string[] Bodies { get; } = new[]
    {
        "Embark on a delightful coding journey as you discover, create, and share awesome script tailored to your app and project preferences.",
        "Browse trending dishes, follow your favorite contributors, and find recipes for any moment of the day.",
        "Step-by-step cooking guidance, video tutorials, and a community that cheers you on as you cook.",
    };

    public string[] Images { get; } = new[]
    {
        "ms-appx:///Assets/Welcome/first_splash_screen.png",
        "ms-appx:///Assets/Welcome/second_splash_screen.png",
        "ms-appx:///Assets/Welcome/third_splash_screen.png",
    };

    [RelayCommand]
    private void Previous()
    {
        if (CurrentPage > 0) CurrentPage--;
    }

    [RelayCommand]
    private async Task Next()
    {
        if (CurrentPage < Titles.Length - 1) CurrentPage++;
        else await Navigator.NavigateRouteAsync(this, "-/Login");
    }

    [RelayCommand]
    private async Task Skip() => await Navigator.NavigateRouteAsync(this, "-/Login");
}

public partial class LoginViewModel : ChefsViewModelBase
{
    public LoginViewModel(INavigator navigator, IDataService data) : base(navigator, data) { }

    [ObservableProperty] private string username = "james.bondi@gmail.com";
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private bool rememberMe = true;

    [RelayCommand]
    private async Task Login() => await Navigator.NavigateRouteAsync(this, "-/Home");

    [RelayCommand]
    private async Task Register() => await Navigator.NavigateViewModelAsync<RegisterViewModel>(this);

    [RelayCommand]
    private async Task SignInApple() => await Navigator.NavigateRouteAsync(this, "-/Home");

    [RelayCommand]
    private async Task SignInGoogle() => await Navigator.NavigateRouteAsync(this, "-/Home");
}

public partial class RegisterViewModel : ChefsViewModelBase
{
    public RegisterViewModel(INavigator navigator, IDataService data) : base(navigator, data) { }

    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;

    [RelayCommand]
    private async Task SignUp() => await Navigator.NavigateRouteAsync(this, "-/Home");

    [RelayCommand]
    private async Task GoToLogin() => await Navigator.NavigateRouteAsync(this, "-/Login");
}

public partial class HomeViewModel : ChefsViewModelBase
{
    public HomeViewModel(INavigator navigator, IDataService data) : base(navigator, data)
    {
        _ = LoadAsync();
    }

    [ObservableProperty] private string greeting = "Welcome back!";
    [ObservableProperty] private string fullName = "Chef";

    public ObservableCollection<Recipe> Trending { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<Recipe> RecentlyAdded { get; } = new();
    public ObservableCollection<User> Contributors { get; } = new();

    private async Task LoadAsync()
    {
        var users = await Data.GetUsersAsync();
        if (users.Count > 0)
        {
            FullName = users[0].FullName;
            Greeting = $"Hello, {users[0].FullName.Split(' ')[0]}";
        }

        var recipes = await Data.GetRecipesAsync();
        foreach (var r in recipes.Take(5)) Trending.Add(r);
        foreach (var r in recipes.OrderByDescending(x => x.Date).Take(8)) RecentlyAdded.Add(r);

        var cats = await Data.GetCategoriesAsync();
        foreach (var c in cats) Categories.Add(c);

        foreach (var u in users.Take(8)) Contributors.Add(u);
    }

    [RelayCommand]
    private async Task OpenRecipe(Recipe? recipe)
    {
        if (recipe is null) return;
        await Navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: recipe);
    }

    [RelayCommand]
    private async Task OpenCategory(Category? category)
    {
        if (category is null) return;
        await Navigator.NavigateViewModelAsync<SearchViewModel>(this, data: category);
    }

    [RelayCommand]
    private async Task OpenContributor(User? user)
    {
        if (user is null) return;
        await Navigator.NavigateViewModelAsync<OtherProfileViewModel>(this, data: user);
    }

    [RelayCommand]
    private async Task OpenNearMe() => await Navigator.NavigateViewModelAsync<NearMeMapViewModel>(this);

    [RelayCommand]
    private async Task OpenNotifications() => await Navigator.NavigateViewModelAsync<NotificationsViewModel>(this);

    [RelayCommand]
    private async Task OpenProfile() => await Navigator.NavigateViewModelAsync<ProfileViewModel>(this);
}

public partial class SearchViewModel : ChefsViewModelBase
{
    public SearchViewModel(INavigator navigator, IDataService data) : base(navigator, data)
    {
        _ = LoadAsync();
    }

    [ObservableProperty] private string query = string.Empty;
    [ObservableProperty] private int resultCount;

    public ObservableCollection<Recipe> Results { get; } = new();
    private List<Recipe>? _all;

    partial void OnQueryChanged(string value) => Refresh();

    private async Task LoadAsync()
    {
        _all = (await Data.GetRecipesAsync()).ToList();
        Refresh();
    }

    private void Refresh()
    {
        if (_all is null) return;
        Results.Clear();
        var q = (Query ?? string.Empty).Trim();
        var filtered = string.IsNullOrEmpty(q)
            ? _all
            : _all.Where(r => r.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                || r.Category.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var r in filtered) Results.Add(r);
        ResultCount = Results.Count;
    }

    [RelayCommand]
    private async Task OpenFilters() => await Navigator.NavigateViewModelAsync<FiltersViewModel>(this);

    [RelayCommand]
    private async Task OpenRecipe(Recipe? recipe)
    {
        if (recipe is null) return;
        await Navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: recipe);
    }
}

public partial class FiltersViewModel : ChefsViewModelBase
{
    public FiltersViewModel(INavigator navigator, IDataService data) : base(navigator, data)
    {
        _ = LoadAsync();
    }

    public ObservableCollection<Category> Categories { get; } = new();
    public string[] CookingTimes { get; } = new[] { "Under 15", "15–30", "30–60", "60+" };
    public string[] SkillLevels { get; } = new[] { "Easy", "Medium", "Hard" };

    [ObservableProperty] private string? selectedCategory;
    [ObservableProperty] private string? selectedCookingTime;
    [ObservableProperty] private string? selectedSkillLevel;

    private async Task LoadAsync()
    {
        var cats = await Data.GetCategoriesAsync();
        foreach (var c in cats) Categories.Add(c);
    }

    [RelayCommand]
    private void Reset()
    {
        SelectedCategory = null;
        SelectedCookingTime = null;
        SelectedSkillLevel = null;
    }

    [RelayCommand]
    private async Task Apply() => await Navigator.NavigateBackAsync(this);

    [RelayCommand]
    private async Task Close() => await Navigator.NavigateBackAsync(this);
}

public partial class RecipeDetailViewModel : ChefsViewModelBase
{
    public RecipeDetailViewModel(INavigator navigator, IDataService data, Recipe? recipe = null) : base(navigator, data)
    {
        Recipe = recipe;
    }

    [ObservableProperty] private Recipe? recipe;
    [ObservableProperty] private int selectedTab;

    public bool HasIngredients => Recipe?.Ingredients.Count > 0;
    public bool HasSteps => Recipe?.Steps.Count > 0;
    public bool HasReviews => (Recipe?.Reviews.Count ?? 0) > 0;
    public bool HasNutrition => !string.IsNullOrWhiteSpace(Recipe?.Calories);

    partial void OnRecipeChanged(Recipe? value)
    {
        OnPropertyChanged(nameof(HasIngredients));
        OnPropertyChanged(nameof(HasSteps));
        OnPropertyChanged(nameof(HasReviews));
        OnPropertyChanged(nameof(HasNutrition));
    }

    [RelayCommand]
    private async Task StartCooking()
    {
        if (Recipe is null) return;
        await Navigator.NavigateViewModelAsync<LiveCookingViewModel>(this, data: Recipe);
    }

    [RelayCommand]
    private void ToggleFavorite()
    {
        if (Recipe is null) return;
        Data.ToggleFavoriteRecipe(Recipe.Id);
        Recipe.Save = Data.FavoriteRecipeIds.Contains(Recipe.Id);
        OnPropertyChanged(nameof(Recipe));
    }

    [RelayCommand]
    private async Task ShowAuthor()
    {
        if (Recipe is null) return;
        await Navigator.NavigateViewModelAsync<OtherProfileViewModel>(this, data: Recipe.Creator);
    }
}

public partial class LiveCookingViewModel : ChefsViewModelBase
{
    public LiveCookingViewModel(INavigator navigator, IDataService data, Recipe? recipe = null) : base(navigator, data)
    {
        Recipe = recipe;
    }

    [ObservableProperty] private Recipe? recipe;
    [ObservableProperty] private int currentStepIndex;

    public Step? CurrentStep => Recipe?.Steps.ElementAtOrDefault(CurrentStepIndex);
    public int TotalSteps => Recipe?.Steps.Count ?? 0;

    partial void OnRecipeChanged(Recipe? value)
    {
        OnPropertyChanged(nameof(CurrentStep));
        OnPropertyChanged(nameof(TotalSteps));
    }

    partial void OnCurrentStepIndexChanged(int value)
    {
        OnPropertyChanged(nameof(CurrentStep));
    }

    [RelayCommand]
    private void Previous() { if (CurrentStepIndex > 0) CurrentStepIndex--; }

    [RelayCommand]
    private async Task Next()
    {
        if (CurrentStepIndex < TotalSteps - 1) CurrentStepIndex++;
        else if (Recipe is not null)
        {
            await Navigator.NavigateViewModelAsync<LiveCookingFinishViewModel>(this, data: Recipe);
        }
    }
}

public partial class LiveCookingFinishViewModel : ChefsViewModelBase
{
    public LiveCookingFinishViewModel(INavigator navigator, IDataService data, Recipe? recipe = null) : base(navigator, data)
    {
        Recipe = recipe;
    }
    [ObservableProperty] private Recipe? recipe;
    [ObservableProperty] private int rating = 5;

    [RelayCommand]
    private async Task ToggleFavorite()
    {
        if (Recipe is null) return;
        Data.ToggleFavoriteRecipe(Recipe.Id);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task Done() => await Navigator.NavigateRouteAsync(this, "-/Home");
}

public partial class FavoritesViewModel : ChefsViewModelBase
{
    public FavoritesViewModel(INavigator navigator, IDataService data) : base(navigator, data)
    {
        _ = LoadAsync();
    }

    [ObservableProperty] private int selectedSegment;

    public ObservableCollection<Recipe> Recipes { get; } = new();
    public ObservableCollection<Cookbook> Cookbooks { get; } = new();

    private async Task LoadAsync()
    {
        var allRecipes = await Data.GetRecipesAsync();
        var favIds = await Data.GetSavedRecipeIdsAsync();
        foreach (var r in allRecipes.Where(r => favIds.Contains(r.Id))) Recipes.Add(r);

        var allCookbooks = await Data.GetCookbooksAsync();
        var savedCb = await Data.GetSavedCookbookIdsAsync();
        foreach (var cb in allCookbooks.Where(cb => savedCb.Contains(cb.Id))) Cookbooks.Add(cb);
    }

    [RelayCommand]
    private async Task OpenRecipe(Recipe? r)
    {
        if (r is null) return;
        await Navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: r);
    }

    [RelayCommand]
    private async Task OpenCookbook(Cookbook? cb)
    {
        if (cb is null) return;
        await Navigator.NavigateViewModelAsync<CookbookDetailViewModel>(this, data: cb);
    }

    [RelayCommand]
    private async Task CreateCookbook() => await Navigator.NavigateViewModelAsync<CreateCookbookViewModel>(this);
}

public partial class CookbookDetailViewModel : ChefsViewModelBase
{
    public CookbookDetailViewModel(INavigator navigator, IDataService data, Cookbook? cookbook = null) : base(navigator, data)
    {
        Cookbook = cookbook;
    }
    [ObservableProperty] private Cookbook? cookbook;

    [RelayCommand]
    private async Task OpenRecipe(Recipe? r)
    {
        if (r is null) return;
        await Navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: r);
    }

    [RelayCommand]
    private async Task Edit()
    {
        if (Cookbook is null) return;
        await Navigator.NavigateViewModelAsync<UpdateCookbookViewModel>(this, data: Cookbook);
    }

    [RelayCommand]
    private async Task AddRecipe() => await Navigator.NavigateViewModelAsync<UpdateCookbookViewModel>(this, data: Cookbook!);
}

public partial class CreateCookbookViewModel : ChefsViewModelBase
{
    public CreateCookbookViewModel(INavigator navigator, IDataService data) : base(navigator, data)
    {
        _ = LoadAsync();
    }
    [ObservableProperty] private string name = string.Empty;
    public ObservableCollection<Recipe> AvailableRecipes { get; } = new();

    private async Task LoadAsync()
    {
        var all = await Data.GetRecipesAsync();
        foreach (var r in all) AvailableRecipes.Add(r);
    }

    [RelayCommand]
    private void ToggleRecipe(Recipe? r)
    {
        if (r is null) return;
        Data.ToggleFavoriteRecipe(r.Id);
    }

    [RelayCommand]
    private async Task Cancel() => await Navigator.NavigateBackAsync(this);

    [RelayCommand]
    private async Task Create() => await Navigator.NavigateBackAsync(this);
}

public partial class UpdateCookbookViewModel : ChefsViewModelBase
{
    public UpdateCookbookViewModel(INavigator navigator, IDataService data, Cookbook? cookbook = null) : base(navigator, data)
    {
        Cookbook = cookbook;
        _ = LoadAsync();
    }
    [ObservableProperty] private Cookbook? cookbook;
    [ObservableProperty] private string name = string.Empty;
    public ObservableCollection<Recipe> AvailableRecipes { get; } = new();

    partial void OnCookbookChanged(Cookbook? value)
    {
        if (value is not null) Name = value.Name;
    }

    private async Task LoadAsync()
    {
        var all = await Data.GetRecipesAsync();
        foreach (var r in all) AvailableRecipes.Add(r);
    }

    [RelayCommand]
    private async Task Cancel() => await Navigator.NavigateBackAsync(this);

    [RelayCommand]
    private async Task Apply() => await Navigator.NavigateBackAsync(this);
}

public partial class ProfileViewModel : ChefsViewModelBase
{
    public ProfileViewModel(INavigator navigator, IDataService data) : base(navigator, data)
    {
        _ = LoadAsync();
    }

    [ObservableProperty] private User? user;
    public ObservableCollection<Recipe> MyRecipes { get; } = new();

    private async Task LoadAsync()
    {
        var users = await Data.GetUsersAsync();
        User = users.FirstOrDefault() ?? new User();
        var recipes = await Data.GetRecipesAsync();
        foreach (var r in recipes.Where(r => r.UserId == User.Id || r.Creator.Id == User.Id)) MyRecipes.Add(r);
    }

    [RelayCommand]
    private async Task OpenSettings() => await Navigator.NavigateViewModelAsync<SettingsViewModel>(this);

    [RelayCommand]
    private async Task OpenNotifications() => await Navigator.NavigateViewModelAsync<NotificationsViewModel>(this);

    [RelayCommand]
    private async Task CreateRecipe() => await Navigator.NavigateViewModelAsync<CreateCookbookViewModel>(this);

    [RelayCommand]
    private async Task OpenRecipe(Recipe? r)
    {
        if (r is null) return;
        await Navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: r);
    }
}

public partial class OtherProfileViewModel : ChefsViewModelBase
{
    public OtherProfileViewModel(INavigator navigator, IDataService data, User? user = null) : base(navigator, data)
    {
        User = user;
    }
    [ObservableProperty] private User? user;
    public ObservableCollection<Recipe> AuthorRecipes { get; } = new();

    partial void OnUserChanged(User? value)
    {
        _ = LoadRecipesAsync();
    }

    private async Task LoadRecipesAsync()
    {
        AuthorRecipes.Clear();
        if (User is null) return;
        var all = await Data.GetRecipesAsync();
        foreach (var r in all.Where(r => r.UserId == User.Id || r.Creator.Id == User.Id))
            AuthorRecipes.Add(r);
    }

    [RelayCommand]
    private async Task OpenRecipe(Recipe? r)
    {
        if (r is null) return;
        await Navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: r);
    }
}

public partial class SettingsViewModel : ChefsViewModelBase
{
    public SettingsViewModel(INavigator navigator, IDataService data)
        : base(navigator, data)
    {
        _ = LoadAsync();
        IsDarkMode = Uno.Toolkit.UI.SystemThemeHelper.GetApplicationTheme() == Microsoft.UI.Xaml.ApplicationTheme.Dark;
    }

    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string mobileNumber = string.Empty;
    [ObservableProperty] private bool notificationsEnabled = true;
    [ObservableProperty] private bool isDarkMode;

    partial void OnIsDarkModeChanged(bool value)
    {
        Uno.Toolkit.UI.SystemThemeHelper.SetApplicationTheme(value);
    }

    private async Task LoadAsync()
    {
        var users = await Data.GetUsersAsync();
        if (users.Count > 0)
        {
            FullName = users[0].FullName;
            Email = users[0].Email;
            MobileNumber = users[0].PhoneNumber ?? string.Empty;
        }
    }

    [RelayCommand]
    private async Task Save() => await Navigator.NavigateBackAsync(this);

    [RelayCommand]
    private async Task LogOut() => await Navigator.NavigateRouteAsync(this, "-/Login");
}

public partial class NotificationsViewModel : ChefsViewModelBase
{
    public NotificationsViewModel(INavigator navigator, IDataService data) : base(navigator, data)
    {
        _ = LoadAsync();
    }

    [ObservableProperty] private int selectedSegment;

    public ObservableCollection<Notification> All { get; } = new();
    public ObservableCollection<Notification> Filtered { get; } = new();

    partial void OnSelectedSegmentChanged(int value) => ApplyFilter();

    private async Task LoadAsync()
    {
        var items = await Data.GetNotificationsAsync();
        foreach (var n in items) All.Add(n);
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        Filtered.Clear();
        IEnumerable<Notification> source = SelectedSegment switch
        {
            1 => All.Where(n => !n.IsRead),
            2 => All.Where(n => n.IsRead),
            _ => All,
        };
        foreach (var n in source.OrderByDescending(n => n.Date)) Filtered.Add(n);
    }

    [RelayCommand]
    private async Task Close() => await Navigator.NavigateBackAsync(this);
}

public partial class NearMeMapViewModel : ChefsViewModelBase
{
    public NearMeMapViewModel(INavigator navigator, IDataService data) : base(navigator, data)
    {
        _ = LoadAsync();
    }

    public ObservableCollection<User> Pins { get; } = new();

    private async Task LoadAsync()
    {
        var users = await Data.GetUsersAsync();
        foreach (var u in users.Take(8)) Pins.Add(u);
    }
}
