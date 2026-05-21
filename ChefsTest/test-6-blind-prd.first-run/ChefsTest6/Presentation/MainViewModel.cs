using System.Collections.ObjectModel;

namespace ChefsTest6.Presentation;

public partial class MainViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefService _chef;

    public HomeTabViewModel Home { get; }
    public SearchTabViewModel Search { get; }
    public FavoritesTabViewModel Favorites { get; }
    public ProfileTabViewModel Profile { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHomeTab), nameof(IsSearchTab), nameof(IsFavoritesTab), nameof(IsProfileTab))]
    private int selectedTab;

    public bool IsHomeTab => SelectedTab == 0;
    public bool IsSearchTab => SelectedTab == 1;
    public bool IsFavoritesTab => SelectedTab == 2;
    public bool IsProfileTab => SelectedTab == 3;

    // Fires when TabBar SelectedIndex changes the bound SelectedTab. Populates the
    // newly-selected tab's data so re-entries refresh too — the prior fix wired
    // Populate() inside each GoToXxx command, but TabBar's TwoWay binding bypasses
    // those commands on user clicks.
    partial void OnSelectedTabChanged(int value)
    {
        switch (value)
        {
            case 0: Home.Populate(); break;
            case 1: Search.Populate(); break;
            case 2: Favorites.Populate(); break;
            case 3: Profile.Populate(); break;
        }
    }

    public INavigator Navigator => _navigator;

    public MainViewModel(INavigator navigator, IChefService chef)
    {
        _navigator = navigator;
        _chef = chef;
        Home = new HomeTabViewModel(this, chef);
        Search = new SearchTabViewModel(this, chef);
        Favorites = new FavoritesTabViewModel(this, chef);
        Profile = new ProfileTabViewModel(this, chef);
    }

    [RelayCommand]
    private void GoToHome()
    {
        SelectedTab = 0;
        Home.Populate();
    }

    [RelayCommand]
    private void GoToSearch()
    {
        SelectedTab = 1;
        Search.Populate();
    }

    [RelayCommand]
    private void GoToFavorites()
    {
        SelectedTab = 2;
        Favorites.Populate();
    }

    [RelayCommand]
    private void GoToProfile()
    {
        SelectedTab = 3;
        Profile.Populate();
    }

    [RelayCommand]
    public async Task OpenRecipeAsync(Recipe? recipe)
    {
        if (recipe is null) return;
        await _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: recipe);
    }

    [RelayCommand]
    public async Task OpenCookbookAsync(Cookbook? cookbook)
    {
        if (cookbook is null) return;
        await _navigator.NavigateViewModelAsync<CookbookDetailViewModel>(this, data: cookbook);
    }

    [RelayCommand]
    public async Task OpenOtherProfileAsync(User? user)
    {
        if (user is null) return;
        await _navigator.NavigateViewModelAsync<OtherProfileViewModel>(this, data: user);
    }

    [RelayCommand]
    public void OpenCategory(Category? category)
    {
        if (category is null) return;
        SelectedTab = 1;
        Search.Query = category.Name ?? string.Empty;
    }

    [RelayCommand]
    public async Task OpenNotificationsAsync()
        => await _navigator.NavigateViewModelAsync<NotificationsViewModel>(this);

    [RelayCommand]
    public async Task OpenSettingsAsync()
        => await _navigator.NavigateViewModelAsync<SettingsViewModel>(this);

    [RelayCommand]
    public async Task OpenMapAsync()
        => await _navigator.NavigateViewModelAsync<MapViewModel>(this);

    [RelayCommand]
    public void ToggleFavorite(Recipe? recipe)
    {
        if (recipe is null) return;
        _chef.ToggleFavorite(recipe);
        Favorites.RefreshSavedRecipes();
    }

    [RelayCommand]
    public async Task CreateCookbookAsync()
        => await _navigator.NavigateViewModelAsync<CreateCookbookViewModel>(this);

    [RelayCommand]
    public async Task EditCookbookAsync(Cookbook? cookbook)
    {
        if (cookbook is null) return;
        await _navigator.NavigateViewModelAsync<EditCookbookViewModel>(this, data: cookbook);
    }
}
