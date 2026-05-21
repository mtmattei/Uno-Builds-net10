using System.Collections.ObjectModel;

namespace ChefsTest1.Presentation;

public partial class FavoritesViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IRecipeService _recipes;
    private readonly ICookbookService _cookbooks;

    public ObservableCollection<Recipe> FavoriteRecipes { get; } = new();
    public ObservableCollection<Cookbook> Cookbooks { get; } = new();

    [ObservableProperty] private int selectedTabIndex; // 0 = recipes, 1 = cookbooks

    public bool IsRecipesTab => SelectedTabIndex == 0;
    public bool IsCookbooksTab => SelectedTabIndex == 1;
    public bool HasFavoriteRecipes => FavoriteRecipes.Count > 0;
    public bool HasNoFavoriteRecipes => FavoriteRecipes.Count == 0;
    public bool HasCookbooks => Cookbooks.Count > 0;
    public bool HasNoCookbooks => Cookbooks.Count == 0;
    public string FavoriteCountLabel => $"{FavoriteRecipes.Count} results";

    public FavoritesViewModel(INavigator navigator, IRecipeService recipes, ICookbookService cookbooks)
    {
        _navigator = navigator;
        _recipes = recipes;
        _cookbooks = cookbooks;
        OpenRecipeCommand = new AsyncRelayCommand<Recipe>(OpenRecipeAsync!);
        OpenCookbookCommand = new AsyncRelayCommand<Cookbook>(OpenCookbookAsync!);
        CreateCookbookCommand = new AsyncRelayCommand(CreateCookbookAsync);
        SelectRecipesCommand = new RelayCommand(() => SelectedTabIndex = 0);
        SelectCookbooksCommand = new AsyncRelayCommand(async () =>
        {
            SelectedTabIndex = 1;
            await ReloadCookbooksAsync();
        });
        OpenNotificationsCommand = new AsyncRelayCommand(OpenNotificationsAsync);
        OpenOwnProfileCommand = new AsyncRelayCommand(OpenOwnProfileAsync);
        _ = LoadAsync();
    }

    public ICommand OpenRecipeCommand { get; }
    public ICommand OpenCookbookCommand { get; }
    public ICommand CreateCookbookCommand { get; }
    public ICommand SelectRecipesCommand { get; }
    public ICommand SelectCookbooksCommand { get; }
    public ICommand OpenNotificationsCommand { get; }
    public ICommand OpenOwnProfileCommand { get; }

    partial void OnSelectedTabIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsRecipesTab));
        OnPropertyChanged(nameof(IsCookbooksTab));
    }

    private async Task LoadAsync()
    {
        var favs = await _recipes.GetFavoritedAsync();
        foreach (var r in favs) FavoriteRecipes.Add(r);
        await ReloadCookbooksAsync();
        OnPropertyChanged(nameof(HasFavoriteRecipes));
        OnPropertyChanged(nameof(HasNoFavoriteRecipes));
        OnPropertyChanged(nameof(FavoriteCountLabel));
    }

    private async Task ReloadCookbooksAsync()
    {
        Cookbooks.Clear();
        var saved = await _cookbooks.GetSavedAsync();
        foreach (var c in saved) Cookbooks.Add(c);
        OnPropertyChanged(nameof(HasCookbooks));
        OnPropertyChanged(nameof(HasNoCookbooks));
    }

    private async Task OpenRecipeAsync(Recipe recipe)
    {
        if (recipe == null) return;
        await _navigator.NavigateRouteAsync(this, "RecipeDetail", data: recipe);
    }

    private async Task OpenCookbookAsync(Cookbook cookbook)
    {
        if (cookbook == null) return;
        await _navigator.NavigateRouteAsync(this, "CookbookDetail", data: cookbook);
    }

    private async Task CreateCookbookAsync()
    {
        await _navigator.NavigateRouteAsync(this, "CreateCookbook");
    }

    private async Task OpenNotificationsAsync()
    {
        await _navigator.NavigateRouteAsync(this, "Notifications");
    }

    private async Task OpenOwnProfileAsync()
    {
        await _navigator.NavigateRouteAsync(this, "Profile");
    }
}
