using System.Collections.ObjectModel;

namespace ChefsTest1.Presentation;

public partial class CreateCookbookViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IRecipeService _recipes;
    private readonly ICookbookService _cookbooks;

    public ObservableCollection<Recipe> AvailableRecipes { get; } = new();
    public string Title { get; protected set; } = "Create Cookbook";
    public string SectionTitle { get; protected set; } = "Add recipes";
    public string ApplyButton { get; protected set; } = "Create cookbook";

    [ObservableProperty] private string cookbookName = string.Empty;

    public CreateCookbookViewModel(INavigator navigator, IRecipeService recipes, ICookbookService cookbooks)
    {
        _navigator = navigator;
        _recipes = recipes;
        _cookbooks = cookbooks;
        CancelCommand = new AsyncRelayCommand(() => _navigator.NavigateBackAsync(this));
        ApplyCommand = new AsyncRelayCommand(ApplyAsync);
        ToggleFavoriteCommand = new AsyncRelayCommand<Recipe>(ToggleAsync!);
        _ = LoadAsync();
    }

    public ICommand CancelCommand { get; }
    public ICommand ApplyCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }

    private async Task LoadAsync()
    {
        var all = await _recipes.GetAllAsync();
        foreach (var r in all.Take(10)) AvailableRecipes.Add(r);
    }

    private async Task ToggleAsync(Recipe r)
    {
        if (r == null) return;
        await _recipes.ToggleFavoriteAsync(r.Id);
        r.IsFavorite = !r.IsFavorite;
    }

    protected virtual async Task ApplyAsync()
    {
        var cookbook = new Cookbook
        {
            Id = Guid.NewGuid(),
            Name = string.IsNullOrWhiteSpace(CookbookName) ? "New Cookbook" : CookbookName,
            Recipes = AvailableRecipes.Where(r => r.IsFavorite).ToList()
        };
        await _cookbooks.SaveAsync(cookbook);
        await _navigator.NavigateBackAsync(this);
    }
}

public partial class UpdateCookbookViewModel : CreateCookbookViewModel
{
    private readonly INavigator _navigator;
    private readonly ICookbookService _cookbooks;

    public Cookbook Cookbook { get; }

    public UpdateCookbookViewModel(INavigator navigator, IRecipeService recipes, ICookbookService cookbooks, Cookbook cookbook)
        : base(navigator, recipes, cookbooks)
    {
        _navigator = navigator;
        _cookbooks = cookbooks;
        Cookbook = cookbook;
        CookbookName = cookbook?.Name ?? string.Empty;
        Title = "Update Cookbook";
        SectionTitle = "Manage cookbook's recipes";
        ApplyButton = "Apply change";
    }

    protected override async Task ApplyAsync()
    {
        Cookbook.Name = CookbookName;
        Cookbook.Recipes = AvailableRecipes.Where(r => r.IsFavorite).ToList();
        await _cookbooks.SaveAsync(Cookbook);
        await _navigator.NavigateBackAsync(this);
    }
}
