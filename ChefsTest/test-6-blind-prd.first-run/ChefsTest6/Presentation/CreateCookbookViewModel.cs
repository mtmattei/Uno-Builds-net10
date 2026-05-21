using System.Collections.ObjectModel;

namespace ChefsTest6.Presentation;

public partial class CookbookRecipePick : ObservableObject
{
    public Recipe Recipe { get; }

    [ObservableProperty]
    private bool isSelected;

    public CookbookRecipePick(Recipe recipe, bool selected = false)
    {
        Recipe = recipe;
        isSelected = selected;
    }
}

public partial class CreateCookbookViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefService _chef;

    public ObservableCollection<CookbookRecipePick> Picks { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreate))]
    private string? cookbookName;

    public bool CanCreate => !string.IsNullOrWhiteSpace(CookbookName);

    public CreateCookbookViewModel(INavigator navigator, IChefService chef)
    {
        _navigator = navigator;
        _chef = chef;
        foreach (var r in chef.AllRecipes) Picks.Add(new CookbookRecipePick(r));
    }

    [RelayCommand]
    private void TogglePick(CookbookRecipePick? pick)
    {
        if (pick is null) return;
        pick.IsSelected = !pick.IsSelected;
    }

    [RelayCommand]
    private async Task CreateAsync()
    {
        if (!CanCreate) return;
        var selected = Picks.Where(p => p.IsSelected).Select(p => p.Recipe);
        _chef.CreateCookbook(CookbookName!.Trim(), selected);
        await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);
    }

    [RelayCommand]
    private async Task CancelAsync() => await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);

    [RelayCommand]
    private async Task BackAsync() => await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);
}
