namespace ChefsTest6.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IRecipeService _recipes;
    private readonly IUserService _users;

    public IReadOnlyList<RecipeData> Trending { get; }
    public IReadOnlyList<CategoryData> Categories { get; }
    public IReadOnlyList<RecipeData> RecentlyAdded { get; }
    public IReadOnlyList<UserData> PopularContributors { get; }
    public UserData CurrentUser { get; }

    public HomeViewModel(INavigator navigator, IRecipeService recipes, IUserService users)
    {
        _navigator = navigator;
        _recipes = recipes;
        _users = users;
        Trending = recipes.Trending();
        Categories = recipes.Categories();
        RecentlyAdded = recipes.RecentlyAdded();
        PopularContributors = users.PopularContributors();
        CurrentUser = users.Current;
    }

    [RelayCommand]
    private async Task OpenRecipe(RecipeData? recipe)
    {
        if (recipe is null) return;
        await _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: recipe);
    }

    [RelayCommand]
    private void ToggleFavorite(RecipeData? recipe)
    {
        if (recipe is null) return;
        _recipes.ToggleFavorite(recipe);
    }

    [RelayCommand]
    private async Task OpenCategory(CategoryData? category)
    {
        if (category is null) return;
        await _navigator.NavigateViewModelAsync<SearchViewModel>(this, data: category);
    }

    [RelayCommand]
    private async Task OpenContributor(UserData? user)
    {
        if (user is null) return;
        await _navigator.NavigateViewModelAsync<OtherProfileViewModel>(this, data: user);
    }

    [RelayCommand]
    private async Task OpenNearMe()
    {
        await _navigator.NavigateViewModelAsync<NearMeMapViewModel>(this);
    }

    [RelayCommand]
    private async Task OpenNotifications()
    {
        await _navigator.NavigateViewModelAsync<NotificationsViewModel>(this);
    }

    [RelayCommand]
    private async Task OpenProfile()
    {
        await _navigator.NavigateViewModelAsync<ProfileViewModel>(this);
    }

    [RelayCommand]
    private async Task ViewAllTrending()
    {
        await _navigator.NavigateViewModelAsync<SearchViewModel>(this, data: "trending");
    }

    [RelayCommand]
    private async Task ViewAllRecent()
    {
        await _navigator.NavigateViewModelAsync<SearchViewModel>(this, data: "recent");
    }
}
