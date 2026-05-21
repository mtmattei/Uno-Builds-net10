using System.Collections.ObjectModel;

namespace ChefsTest6.ViewModels;

public partial class UpdateCookbookViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly ICookbookService _cookbooks;
    private readonly IRecipeService _recipes;
    private readonly CookbookData _cookbook;

    [ObservableProperty]
    private string cookbookName;

    public ObservableCollection<RecipePickerEntry> Picker { get; } = new();

    public UpdateCookbookViewModel(INavigator navigator, ICookbookService cookbooks, IRecipeService recipes, CookbookData cookbook)
    {
        _navigator = navigator;
        _cookbooks = cookbooks;
        _recipes = recipes;
        _cookbook = cookbook;
        cookbookName = cookbook.Name ?? string.Empty;
        var selectedIds = new HashSet<Guid>(cookbook.Recipes.Select(r => r.Id));
        foreach (var r in recipes.All())
        {
            Picker.Add(new RecipePickerEntry(r) { IsSelected = selectedIds.Contains(r.Id) });
        }
    }

    public bool CanSave => !string.IsNullOrWhiteSpace(CookbookName);

    partial void OnCookbookNameChanged(string value) => OnPropertyChanged(nameof(CanSave));

    [RelayCommand]
    private void Toggle(RecipePickerEntry? entry)
    {
        if (entry is null) return;
        entry.IsSelected = !entry.IsSelected;
    }

    [RelayCommand]
    private async Task Apply()
    {
        if (!CanSave) return;
        var selected = Picker.Where(p => p.IsSelected).Select(p => p.Recipe).ToList();
        _cookbooks.Update(_cookbook, CookbookName, selected);
        await _navigator.NavigateBackAsync(this);
    }

    [RelayCommand]
    private async Task Cancel() => await _navigator.NavigateBackAsync(this);
}
