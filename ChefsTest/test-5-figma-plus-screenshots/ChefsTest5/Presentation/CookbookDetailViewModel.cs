namespace ChefsTest5.Presentation;

public partial class CookbookDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty] private CookbookData cookbook = new();
    public int RecipeCount => Cookbook.Recipes?.Count ?? 0;
    public IAsyncRelayCommand BackCommand { get; }

    public CookbookDetailViewModel(INavigator navigator, CookbookData? data = null)
    {
        _navigator = navigator;
        if (data is not null) Cookbook = data;
        BackCommand = new AsyncRelayCommand(async () => await _navigator.NavigateBackAsync(this));
    }

    partial void OnCookbookChanged(CookbookData value) => OnPropertyChanged(nameof(RecipeCount));
}
