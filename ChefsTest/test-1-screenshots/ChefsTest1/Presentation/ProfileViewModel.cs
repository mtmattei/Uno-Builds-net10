using System.Collections.ObjectModel;

namespace ChefsTest1.Presentation;

public partial class ProfileViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IUserService _users;
    private readonly IRecipeService _recipes;

    public ObservableCollection<Recipe> MyRecipes { get; } = new();

    [ObservableProperty] private User? user;

    public bool HasRecipes => MyRecipes.Count > 0;
    public bool HasNoRecipes => MyRecipes.Count == 0;

    public ProfileViewModel(INavigator navigator, IUserService users, IRecipeService recipes)
    {
        _navigator = navigator;
        _users = users;
        _recipes = recipes;
        BackCommand = new AsyncRelayCommand(() => _navigator.NavigateBackAsync(this));
        SettingsCommand = new AsyncRelayCommand(() => _navigator.NavigateRouteAsync(this, "Settings"));
        OpenRecipeCommand = new AsyncRelayCommand<Recipe>(r => _navigator.NavigateRouteAsync(this, "RecipeDetail", data: r!)!);
        CreateRecipeCommand = new RelayCommand(() => { });
        _ = LoadAsync();
    }

    public ICommand BackCommand { get; }
    public ICommand SettingsCommand { get; }
    public ICommand OpenRecipeCommand { get; }
    public ICommand CreateRecipeCommand { get; }

    private async Task LoadAsync()
    {
        var u = await _users.GetCurrentAsync() ?? new User
        {
            FullName = "Jenna Smith",
            Recipes = 12,
            Followers = 156,
            Following = 3127,
            UrlProfileImage = "ms-appx:///Assets/Profiles/niki_samantha.png"
        };
        u.FullName ??= "Jenna Smith";
        u.Recipes ??= 12;
        u.Followers ??= 156;
        u.Following ??= 3127;
        User = u;
        var all = await _recipes.GetAllAsync();
        foreach (var r in all.Take((int)(u.Recipes ?? 0))) MyRecipes.Add(r);
        OnPropertyChanged(nameof(HasRecipes));
        OnPropertyChanged(nameof(HasNoRecipes));
    }
}
