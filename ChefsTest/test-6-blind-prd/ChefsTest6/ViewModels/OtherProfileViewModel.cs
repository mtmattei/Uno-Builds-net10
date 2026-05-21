namespace ChefsTest6.ViewModels;

public partial class OtherProfileViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IRecipeService _recipes;

    public UserData User { get; }
    public IReadOnlyList<RecipeData> Recipes { get; }

    public OtherProfileViewModel(INavigator navigator, IRecipeService recipes, UserData user)
    {
        _navigator = navigator;
        _recipes = recipes;
        User = user;
        Recipes = recipes.All().Where(r => r.UserId == user.Id).ToList();
    }

    public bool HasRecipes => Recipes.Count > 0;

    [RelayCommand]
    private async Task OpenRecipe(RecipeData? r)
    {
        if (r is null) return;
        await _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: r);
    }
}
