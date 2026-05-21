using System.Collections.ObjectModel;
using System.Linq;

namespace ChefsTest5.Presentation;

public partial class ProfileViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private UserData user = new() { FullName = "Jenna Smith", Recipes = 12, Followers = 156, Following = 3127 };
    public ObservableCollection<RecipeData> MyRecipes { get; } = new();

    public IAsyncRelayCommand BackCommand { get; }
    public IAsyncRelayCommand GoSettingsCommand { get; }

    public ProfileViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        BackCommand = new AsyncRelayCommand(async () => await _navigator.NavigateBackAsync(this));
        GoSettingsCommand = new AsyncRelayCommand(async () =>
            await _navigator.NavigateViewModelAsync<SettingsViewModel>(this));
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var current = await _data.GetCurrentUserAsync();
        if (current is not null) User = current;
        var recipes = await _data.GetRecipesAsync();
        foreach (var r in recipes.Take(8)) MyRecipes.Add(r);
    }
}
