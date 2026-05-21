using System.Collections.ObjectModel;

namespace ChefsTest1.Presentation;

public partial class HomeViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IRecipeService _recipes;
    private readonly IUserService _users;

    public ObservableCollection<Recipe> Trending { get; } = new();
    public ObservableCollection<Recipe> RecentlyAdded { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<User> PopularContributors { get; } = new();

    public HomeViewModel(INavigator navigator, IRecipeService recipes, IUserService users)
    {
        _navigator = navigator;
        _recipes = recipes;
        _users = users;
        OpenRecipeCommand = new AsyncRelayCommand<Recipe>(OpenRecipeAsync!);
        OpenProfileCommand = new AsyncRelayCommand<User>(OpenProfileAsync!);
        OpenNotificationsCommand = new AsyncRelayCommand(OpenNotificationsAsync);
        OpenOwnProfileCommand = new AsyncRelayCommand(OpenOwnProfileAsync);
        ToggleFavoriteCommand = new AsyncRelayCommand<Recipe>(ToggleFavoriteAsync!);
        ViewAllTrendingCommand = new AsyncRelayCommand(ViewAllAsync);
        ViewAllRecentCommand = new AsyncRelayCommand(ViewAllAsync);
        NearMeCommand = new AsyncRelayCommand(GoNearMeAsync);
        OpenCategoryCommand = new AsyncRelayCommand<Category>(OpenCategoryAsync!);
        _ = LoadAsync();
    }

    public ICommand OpenRecipeCommand { get; }
    public ICommand OpenProfileCommand { get; }
    public ICommand OpenNotificationsCommand { get; }
    public ICommand OpenOwnProfileCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand ViewAllTrendingCommand { get; }
    public ICommand ViewAllRecentCommand { get; }
    public ICommand NearMeCommand { get; }
    public ICommand OpenCategoryCommand { get; }

    private async Task LoadAsync()
    {
        try
        {
            var trending = await _recipes.GetTrendingAsync();
            foreach (var r in trending) Trending.Add(r);
            var recent = await _recipes.GetAllAsync();
            foreach (var r in recent.Skip(3).Take(8)) RecentlyAdded.Add(r);
            var cats = await _recipes.GetCategoriesAsync();
            foreach (var c in cats) Categories.Add(c);
            var creators = await _users.GetPopularCreatorsAsync();
            foreach (var u in creators) PopularContributors.Add(u);
        }
        catch { }
    }

    private async Task OpenRecipeAsync(Recipe recipe)
    {
        if (recipe == null) return;
        await _navigator.NavigateRouteAsync(this, "RecipeDetail", data: recipe);
    }

    private async Task OpenProfileAsync(User user)
    {
        if (user == null) return;
        await _navigator.NavigateRouteAsync(this, "OtherProfile", data: user);
    }

    private async Task OpenNotificationsAsync()
    {
        await _navigator.NavigateRouteAsync(this, "Notifications");
    }

    private async Task OpenOwnProfileAsync()
    {
        await _navigator.NavigateRouteAsync(this, "Profile");
    }

    private async Task ToggleFavoriteAsync(Recipe recipe)
    {
        if (recipe == null) return;
        await _recipes.ToggleFavoriteAsync(recipe.Id);
        recipe.IsFavorite = !recipe.IsFavorite;
        OnPropertyChanged(nameof(Trending));
    }

    private async Task ViewAllAsync()
    {
        await _navigator.NavigateRouteAsync(this, "./Search");
    }

    private async Task GoNearMeAsync()
    {
        await _navigator.NavigateRouteAsync(this, "NearMe");
    }

    private async Task OpenCategoryAsync(Category category)
    {
        await _navigator.NavigateRouteAsync(this, "./Search");
    }
}
