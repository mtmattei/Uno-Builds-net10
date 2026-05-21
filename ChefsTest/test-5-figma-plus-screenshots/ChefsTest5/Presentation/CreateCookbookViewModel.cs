using System.Collections.ObjectModel;
using System.Linq;

namespace ChefsTest5.Presentation;

public partial class CreateCookbookViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    public string Title => "Create Cookbook";
    public string PrimaryLabel => "Create cookbook";

    [ObservableProperty] private string cookbookName = string.Empty;
    public ObservableCollection<RecipeData> Recipes { get; } = new();

    public IAsyncRelayCommand BackCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }

    public CreateCookbookViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        BackCommand = new AsyncRelayCommand(async () => await _navigator.NavigateBackAsync(this));
        SaveCommand = new AsyncRelayCommand(async () => await _navigator.NavigateBackAsync(this));
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var recipes = await _data.GetRecipesAsync();
        foreach (var r in recipes.Take(8)) Recipes.Add(r);
    }
}

public partial class UpdateCookbookViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public string Title => "Update cookbook";
    public string PrimaryLabel => "Apply changes";

    [ObservableProperty] private string cookbookName = "Mom's favorite";
    public ObservableCollection<RecipeData> Recipes { get; } = new();

    public IAsyncRelayCommand BackCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }

    public UpdateCookbookViewModel(INavigator navigator, IChefsDataService data, CookbookData? entry = null)
    {
        _navigator = navigator;
        if (entry?.Name is not null) CookbookName = entry.Name;
        if (entry?.Recipes is not null) foreach (var r in entry.Recipes) Recipes.Add(r);
        else _ = LoadAsync(data);
        BackCommand = new AsyncRelayCommand(async () => await _navigator.NavigateBackAsync(this));
        SaveCommand = new AsyncRelayCommand(async () => await _navigator.NavigateBackAsync(this));
    }

    private async Task LoadAsync(IChefsDataService data)
    {
        var recipes = await data.GetRecipesAsync();
        foreach (var r in recipes.Take(8)) Recipes.Add(r);
    }
}
