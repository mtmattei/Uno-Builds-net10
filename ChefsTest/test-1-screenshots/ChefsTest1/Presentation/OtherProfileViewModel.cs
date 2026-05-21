using System.Collections.ObjectModel;

namespace ChefsTest1.Presentation;

public partial class OtherProfileViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IRecipeService _recipes;

    public User User { get; }
    public ObservableCollection<Recipe> TheirRecipes { get; } = new();

    public OtherProfileViewModel(INavigator navigator, IRecipeService recipes, User user)
    {
        _navigator = navigator;
        _recipes = recipes;
        User = user;
        BackCommand = new AsyncRelayCommand(() => _navigator.NavigateBackAsync(this));
        OpenRecipeCommand = new AsyncRelayCommand<Recipe>(r => _navigator.NavigateRouteAsync(this, "RecipeDetail", data: r!)!);
        _ = LoadAsync();
    }

    public ICommand BackCommand { get; }
    public ICommand OpenRecipeCommand { get; }

    private async Task LoadAsync()
    {
        var all = await _recipes.GetAllAsync();
        foreach (var r in all.Where(r => r.UserId == User.Id).Take(8)) TheirRecipes.Add(r);
        if (TheirRecipes.Count == 0)
        {
            foreach (var r in all.Take(6)) TheirRecipes.Add(r);
        }
    }
}
