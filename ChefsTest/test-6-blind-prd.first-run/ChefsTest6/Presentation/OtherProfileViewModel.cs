using System.Collections.ObjectModel;

namespace ChefsTest6.Presentation;

public partial class OtherProfileViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefService _chef;

    public User User { get; }
    public ObservableCollection<Recipe> Recipes { get; } = new();

    [ObservableProperty]
    private bool isFollowing;

    public string FullName => User.FullName ?? "User";
    public string Description => User.Description ?? string.Empty;
    public string Avatar => User.UrlProfileImage ?? "ms-appx:///Assets/Profiles/james_bondi.png";
    public string FollowersText => User.Followers?.ToString() ?? "0";
    public string FollowingText => User.Following?.ToString() ?? "0";
    public string RecipesText => User.Recipes?.ToString() ?? "0";
    public bool IsEmpty => Recipes.Count == 0;

    public OtherProfileViewModel(INavigator navigator, IChefService chef, User user)
    {
        _navigator = navigator;
        _chef = chef;
        User = user;
        foreach (var r in chef.GetRecipesByAuthor(user.Id)) Recipes.Add(r);
    }

    [RelayCommand]
    private void ToggleFollow() => IsFollowing = !IsFollowing;

    [RelayCommand]
    private async Task OpenRecipeAsync(Recipe? recipe)
    {
        if (recipe is null) return;
        await _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: recipe);
    }

    [RelayCommand]
    private void ToggleFavorite(Recipe? recipe)
    {
        if (recipe is null) return;
        _chef.ToggleFavorite(recipe);
    }

    [RelayCommand]
    private async Task BackAsync() => await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);
}
