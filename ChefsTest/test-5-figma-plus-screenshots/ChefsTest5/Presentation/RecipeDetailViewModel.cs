namespace ChefsTest5.Presentation;

public partial class RecipeDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty] private RecipeData recipe = new();
    [ObservableProperty] private int selectedTab;

    public string CookTimeText => $"{(int)Recipe.CookTime.TotalMinutes} mins";
    public string DifficultyText => Recipe.Difficulty switch { 0 => "Easy", 1 => "Medium", _ => "Hard" };
    public int IngredientCount => Recipe.Ingredients?.Count ?? 0;
    public int ReviewCount => Recipe.Reviews?.Count ?? 0;
    public Microsoft.UI.Xaml.Visibility HasReviews => ReviewCount > 0 ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
    public Microsoft.UI.Xaml.Visibility NoReviews => ReviewCount == 0 ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
    public string ProteinText => Recipe.Nutrition is null ? "—" : $"{Recipe.Nutrition.Protein:0} / {Recipe.Nutrition.ProteinBase:0} g";
    public string CarbsText => Recipe.Nutrition is null ? "—" : $"{Recipe.Nutrition.Carbs:0} / {Recipe.Nutrition.CarbsBase:0} g";
    public string FatText => Recipe.Nutrition is null ? "—" : $"{Recipe.Nutrition.Fat:0} / {Recipe.Nutrition.FatBase:0} g";

    public IRelayCommand<string> SelectTabCommand { get; }
    public IAsyncRelayCommand BackCommand { get; }
    public IAsyncRelayCommand StartCookingCommand { get; }

    public RecipeDetailViewModel(INavigator navigator, RecipeData? data = null)
    {
        _navigator = navigator;
        if (data is not null) Recipe = data;
        SelectTabCommand = new RelayCommand<string>(s => { if (int.TryParse(s, out var i)) SelectedTab = i; });
        BackCommand = new AsyncRelayCommand(async () => await _navigator.NavigateBackAsync(this));
        StartCookingCommand = new AsyncRelayCommand(async () =>
            await _navigator.NavigateViewModelAsync<LiveCookingViewModel>(this, data: Recipe));
    }

    partial void OnRecipeChanged(RecipeData value)
    {
        OnPropertyChanged(nameof(CookTimeText));
        OnPropertyChanged(nameof(DifficultyText));
        OnPropertyChanged(nameof(IngredientCount));
        OnPropertyChanged(nameof(ReviewCount));
        OnPropertyChanged(nameof(HasReviews));
        OnPropertyChanged(nameof(NoReviews));
        OnPropertyChanged(nameof(ProteinText));
        OnPropertyChanged(nameof(CarbsText));
        OnPropertyChanged(nameof(FatText));
    }
}
