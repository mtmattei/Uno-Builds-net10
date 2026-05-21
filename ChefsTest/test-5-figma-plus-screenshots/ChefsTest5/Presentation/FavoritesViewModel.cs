using System.Collections.ObjectModel;

namespace ChefsTest5.Presentation;

public partial class FavoritesViewModel : ObservableObject
{
    private readonly IChefsDataService _data;
    private readonly INavigator _navigator;

    [ObservableProperty] private int selectedSegment;

    public ObservableCollection<RecipeData> Recipes { get; } = new();
    public ObservableCollection<CookbookData> Cookbooks { get; } = new();
    public int RecipeCount => Recipes.Count;
    public int CookbookCount => Cookbooks.Count;

    public IRelayCommand<string> SelectSegmentCommand { get; }
    public IAsyncRelayCommand GoProfileCommand { get; }
    public IAsyncRelayCommand GoNotificationsCommand { get; }
    public IAsyncRelayCommand CreateCookbookCommand { get; }

    public FavoritesViewModel(IChefsDataService data, INavigator navigator)
    {
        _data = data;
        _navigator = navigator;
        SelectSegmentCommand = new RelayCommand<string>(s => { if (int.TryParse(s, out var i)) SelectedSegment = i; });
        GoProfileCommand = new AsyncRelayCommand(async () =>
            await _navigator.NavigateViewModelAsync<ProfileViewModel>(this));
        GoNotificationsCommand = new AsyncRelayCommand(async () =>
            await _navigator.NavigateViewModelAsync<NotificationsViewModel>(this));
        CreateCookbookCommand = new AsyncRelayCommand(async () =>
            await _navigator.NavigateViewModelAsync<CreateCookbookViewModel>(this));
    }

    public async Task LoadAsync()
    {
        if (Recipes.Count > 0) return;
        var recipes = await _data.GetRecipesAsync();
        var cookbooks = await _data.GetCookbooksAsync();
        foreach (var r in recipes) Recipes.Add(r);
        foreach (var c in cookbooks) Cookbooks.Add(c);
        OnPropertyChanged(nameof(RecipeCount));
        OnPropertyChanged(nameof(CookbookCount));
    }
}
