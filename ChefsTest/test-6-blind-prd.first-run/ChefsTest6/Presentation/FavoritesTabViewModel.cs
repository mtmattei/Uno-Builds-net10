using System.Collections.ObjectModel;

namespace ChefsTest6.Presentation;

public partial class FavoritesTabViewModel : ObservableObject
{
    private readonly MainViewModel _parent;
    private readonly IChefService _chef;

    public ObservableCollection<Recipe> SavedRecipes { get; } = new();
    public ObservableCollection<Cookbook> Cookbooks { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRecipesTab), nameof(IsCookbooksTab),
        nameof(IsRecipesEmpty), nameof(IsCookbooksEmpty),
        nameof(RecipesText), nameof(CookbooksText))]
    private int segmentIndex;

    public bool IsRecipesTab => SegmentIndex == 0;
    public bool IsCookbooksTab => SegmentIndex == 1;
    public bool IsRecipesEmpty => SavedRecipes.Count == 0;
    public bool IsCookbooksEmpty => Cookbooks.Count == 0;

    public string RecipesText => $"{SavedRecipes.Count} {(SavedRecipes.Count == 1 ? "recipe" : "recipes")}";
    public string CookbooksText => $"{Cookbooks.Count} {(Cookbooks.Count == 1 ? "cookbook" : "cookbooks")}";

    public FavoritesTabViewModel(MainViewModel parent, IChefService chef)
    {
        _parent = parent;
        _chef = chef;
    }

    public void Populate()
    {
        RefreshSavedRecipes();
        RefreshCookbooks();
    }

    public void RefreshSavedRecipes()
    {
        SavedRecipes.Clear();
        foreach (var r in _chef.Favorites) SavedRecipes.Add(r);
        OnPropertyChanged(nameof(IsRecipesEmpty));
        OnPropertyChanged(nameof(RecipesText));
    }

    public void RefreshCookbooks()
    {
        Cookbooks.Clear();
        foreach (var c in _chef.AllCookbooks) Cookbooks.Add(c);
        OnPropertyChanged(nameof(IsCookbooksEmpty));
        OnPropertyChanged(nameof(CookbooksText));
    }

    [RelayCommand]
    private void SelectRecipes() => SegmentIndex = 0;

    [RelayCommand]
    private void SelectCookbooks() => SegmentIndex = 1;

    [RelayCommand]
    private void GoDiscoverPopular() => _parent.GoToHomeCommand.Execute(null);

    public IAsyncRelayCommand<Recipe?> OpenRecipeCommand => _parent.OpenRecipeCommand;
    public IRelayCommand<Recipe?> ToggleFavoriteCommand => _parent.ToggleFavoriteCommand;
    public IAsyncRelayCommand<Cookbook?> OpenCookbookCommand => _parent.OpenCookbookCommand;
    public IAsyncRelayCommand CreateCookbookCommand => _parent.CreateCookbookCommand;
}
