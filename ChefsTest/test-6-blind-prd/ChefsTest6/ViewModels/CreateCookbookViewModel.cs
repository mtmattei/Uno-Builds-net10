using System.Collections.ObjectModel;

namespace ChefsTest6.ViewModels;

public partial class CreateCookbookViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly ICookbookService _cookbooks;
    private readonly IRecipeService _recipes;

    [ObservableProperty]
    private string cookbookName = string.Empty;

    public ObservableCollection<RecipePickerEntry> Picker { get; } = new();

    public CreateCookbookViewModel(INavigator navigator, ICookbookService cookbooks, IRecipeService recipes)
    {
        _navigator = navigator;
        _cookbooks = cookbooks;
        _recipes = recipes;
        foreach (var r in recipes.All())
        {
            Picker.Add(new RecipePickerEntry(r));
        }
    }

    public bool CanSave => !string.IsNullOrWhiteSpace(CookbookName) && Picker.Any(p => p.IsSelected);

    partial void OnCookbookNameChanged(string value) => OnPropertyChanged(nameof(CanSave));

    [RelayCommand]
    private void Toggle(RecipePickerEntry? entry)
    {
        if (entry is null) return;
        entry.IsSelected = !entry.IsSelected;
        OnPropertyChanged(nameof(CanSave));
    }

    [RelayCommand]
    private async Task Save()
    {
        if (!CanSave) return;
        var selected = Picker.Where(p => p.IsSelected).Select(p => p.Recipe).ToList();
        _cookbooks.Create(CookbookName, selected);
        await _navigator.NavigateBackAsync(this);
    }

    [RelayCommand]
    private async Task Cancel() => await _navigator.NavigateBackAsync(this);
}

public partial class RecipePickerEntry : ObservableObject
{
    public RecipeData Recipe { get; }

    [ObservableProperty]
    private bool isSelected;

    public RecipePickerEntry(RecipeData recipe)
    {
        Recipe = recipe;
    }
}
