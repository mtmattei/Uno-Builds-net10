namespace ChefsTest5.Presentation;

public partial class LiveCookingFinishViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private RecipeData recipe = new();
    public string RecipeName => Recipe.Name ?? string.Empty;

    public IAsyncRelayCommand BackCommand { get; }
    public IAsyncRelayCommand FavoriteCommand { get; }

    public LiveCookingFinishViewModel(INavigator navigator, IChefsDataService data, RecipeData? entry = null)
    {
        _navigator = navigator;
        _data = data;
        if (entry is not null) Recipe = entry;
        BackCommand = new AsyncRelayCommand(async () => await _navigator.NavigateBackAsync(this));
        FavoriteCommand = new AsyncRelayCommand(async () => await _data.ToggleFavoriteAsync(Recipe.Id));
    }

    partial void OnRecipeChanged(RecipeData value) => OnPropertyChanged(nameof(RecipeName));
}
