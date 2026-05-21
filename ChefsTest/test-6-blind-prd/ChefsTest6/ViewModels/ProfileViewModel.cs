namespace ChefsTest6.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IUserService _users;
    private readonly IRecipeService _recipes;

    public UserData User { get; }
    public IReadOnlyList<RecipeData> MyRecipes { get; }

    public ProfileViewModel(INavigator navigator, IUserService users, IRecipeService recipes)
    {
        _navigator = navigator;
        _users = users;
        _recipes = recipes;
        User = users.Current;
        MyRecipes = recipes.All().Where(r => r.UserId == User.Id).ToList();
    }

    public bool HasRecipes => MyRecipes.Count > 0;
    public bool ShowSettings => true;

    [RelayCommand]
    private async Task OpenSettings() => await _navigator.NavigateViewModelAsync<SettingsViewModel>(this);

    [RelayCommand]
    private async Task OpenRecipe(RecipeData? r)
    {
        if (r is null) return;
        await _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: r);
    }

    [RelayCommand]
    private async Task OpenSearch() => await _navigator.NavigateViewModelAsync<SearchViewModel>(this);
}
