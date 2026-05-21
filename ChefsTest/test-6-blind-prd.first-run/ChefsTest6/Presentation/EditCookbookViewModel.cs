using System.Collections.ObjectModel;

namespace ChefsTest6.Presentation;

public partial class EditCookbookViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefService _chef;

    public Cookbook Cookbook { get; }

    public ObservableCollection<CookbookRecipePick> Picks { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    private string? cookbookName;

    public bool CanApply => !string.IsNullOrWhiteSpace(CookbookName);

    public EditCookbookViewModel(INavigator navigator, IChefService chef, Cookbook cookbook)
    {
        _navigator = navigator;
        _chef = chef;
        Cookbook = cookbook;
        CookbookName = cookbook.Name;

        var inSet = new HashSet<Guid>(cookbook.Recipes.Select(r => r.Id));
        foreach (var r in chef.AllRecipes)
        {
            Picks.Add(new CookbookRecipePick(r, inSet.Contains(r.Id)));
        }
    }

    [RelayCommand]
    private void TogglePick(CookbookRecipePick? pick)
    {
        if (pick is null) return;
        pick.IsSelected = !pick.IsSelected;
    }

    [RelayCommand]
    private async Task ApplyAsync()
    {
        if (!CanApply) return;
        var selected = Picks.Where(p => p.IsSelected).Select(p => p.Recipe);
        _chef.UpdateCookbook(Cookbook, CookbookName!.Trim(), selected);
        await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);
    }

    [RelayCommand]
    private async Task CancelAsync() => await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);

    [RelayCommand]
    private async Task BackAsync() => await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);
}
