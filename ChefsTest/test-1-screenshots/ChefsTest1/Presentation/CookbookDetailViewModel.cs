using System.Collections.ObjectModel;

namespace ChefsTest1.Presentation;

public partial class CookbookDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public Cookbook Cookbook { get; }
    public ObservableCollection<Recipe> Recipes { get; } = new();
    public string CountLabel => $"{Recipes.Count} results";

    public CookbookDetailViewModel(INavigator navigator, Cookbook cookbook)
    {
        _navigator = navigator;
        Cookbook = cookbook;
        if (cookbook?.Recipes != null) foreach (var r in cookbook.Recipes) Recipes.Add(r);
        BackCommand = new AsyncRelayCommand(() => _navigator.NavigateBackAsync(this));
        OpenRecipeCommand = new AsyncRelayCommand<Recipe>(r => _navigator.NavigateRouteAsync(this, "RecipeDetail", data: r!)!);
        UpdateCommand = new AsyncRelayCommand(() => _navigator.NavigateRouteAsync(this, "UpdateCookbook", data: cookbook!));
    }

    public ICommand BackCommand { get; }
    public ICommand OpenRecipeCommand { get; }
    public ICommand UpdateCommand { get; }
}
