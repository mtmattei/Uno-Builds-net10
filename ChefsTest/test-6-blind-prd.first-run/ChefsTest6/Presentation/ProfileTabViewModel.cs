using System.Collections.ObjectModel;

namespace ChefsTest6.Presentation;

public partial class ProfileTabViewModel : ObservableObject
{
    private readonly MainViewModel _parent;
    private readonly IChefService _chef;

    public ObservableCollection<Recipe> MyRecipes { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty), nameof(MyRecipesCountText))]
    private int recipeCount;

    public User? Me => _chef.CurrentUser;
    public string FullName => Me?.FullName ?? "Guest";
    public string FollowersText => Me?.Followers?.ToString() ?? "0";
    public string FollowingText => Me?.Following?.ToString() ?? "0";
    public string RecipesAuthoredText => Me?.Recipes?.ToString() ?? "0";
    public string Avatar => Me?.UrlProfileImage ?? "ms-appx:///Assets/Profiles/james_bondi.png";
    public bool IsEmpty => MyRecipes.Count == 0;
    public string MyRecipesCountText => $"{MyRecipes.Count} {(MyRecipes.Count == 1 ? "recipe" : "recipes")}";

    public ProfileTabViewModel(MainViewModel parent, IChefService chef)
    {
        _parent = parent;
        _chef = chef;
    }

    public void Populate()
    {
        MyRecipes.Clear();
        if (_chef.CurrentUser is { } u)
        {
            foreach (var r in _chef.GetRecipesByAuthor(u.Id)) MyRecipes.Add(r);
        }
        RecipeCount = MyRecipes.Count;
        OnPropertyChanged(nameof(Me));
        OnPropertyChanged(nameof(FullName));
        OnPropertyChanged(nameof(FollowersText));
        OnPropertyChanged(nameof(FollowingText));
        OnPropertyChanged(nameof(RecipesAuthoredText));
        OnPropertyChanged(nameof(Avatar));
    }

    public IAsyncRelayCommand<Recipe?> OpenRecipeCommand => _parent.OpenRecipeCommand;
    public IRelayCommand<Recipe?> ToggleFavoriteCommand => _parent.ToggleFavoriteCommand;
    public IAsyncRelayCommand OpenSettingsCommand => _parent.OpenSettingsCommand;
    public IAsyncRelayCommand CreateCookbookCommand => _parent.CreateCookbookCommand;
}
