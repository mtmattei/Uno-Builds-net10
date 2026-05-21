using System.Collections.ObjectModel;

namespace ChefsTest6.Presentation;

public partial class RecipeDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefService _chef;

    public Recipe Recipe { get; }

    public ObservableCollection<Review> Reviews { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsIngredientsTab), nameof(IsStepsTab),
        nameof(IsReviewsTab), nameof(IsNutritionTab))]
    private int selectedTabIndex;

    public bool IsIngredientsTab => SelectedTabIndex == 0;
    public bool IsStepsTab => SelectedTabIndex == 1;
    public bool IsReviewsTab => SelectedTabIndex == 2;
    public bool IsNutritionTab => SelectedTabIndex == 3;

    public string IngredientsCountText => $"{Recipe.Ingredients.Count} items";
    public string StepsCountText => $"{Recipe.Steps.Count} steps";
    public string ReviewsCountText => $"{Reviews.Count} comments";
    public bool HasReviews => Reviews.Count > 0;

    public string CookTimeText => Recipe.CookTimeText;
    public string DifficultyText => Recipe.DifficultyText;
    public string CaloriesText => Recipe.Calories ?? "—";
    public string AuthorName => Recipe.Creator?.FullName ?? "Unknown";
    public string AuthorAvatar => Recipe.Creator?.UrlProfileImage ?? "ms-appx:///Assets/Profiles/james_bondi.png";

    public double NutritionProtein => Recipe.Nutrition?.Protein ?? 0;
    public double NutritionProteinMax => Recipe.Nutrition?.ProteinBase ?? 1;
    public double NutritionCarbs => Recipe.Nutrition?.Carbs ?? 0;
    public double NutritionCarbsMax => Recipe.Nutrition?.CarbsBase ?? 1;
    public double NutritionFat => Recipe.Nutrition?.Fat ?? 0;
    public double NutritionFatMax => Recipe.Nutrition?.FatBase ?? 1;

    public string ProteinLabel => $"{NutritionProtein:0} / {NutritionProteinMax:0}g";
    public string CarbsLabel => $"{NutritionCarbs:0} / {NutritionCarbsMax:0}g";
    public string FatLabel => $"{NutritionFat:0} / {NutritionFatMax:0}g";

    public RecipeDetailViewModel(INavigator navigator, IChefService chef, Recipe recipe)
    {
        _navigator = navigator;
        _chef = chef;
        Recipe = recipe;

        foreach (var r in chef.GetReviews(recipe.Id)) Reviews.Add(r);
    }

    [RelayCommand] private void SelectIngredients() => SelectedTabIndex = 0;
    [RelayCommand] private void SelectSteps() => SelectedTabIndex = 1;
    [RelayCommand] private void SelectReviews() => SelectedTabIndex = 2;
    [RelayCommand] private void SelectNutrition() => SelectedTabIndex = 3;

    [RelayCommand]
    private void ToggleFavorite() => _chef.ToggleFavorite(Recipe);

    [RelayCommand]
    private void ToggleLike(Review? review)
    {
        if (review is null) return;
        _chef.ToggleLike(review);
    }

    [RelayCommand]
    private void ToggleDislike(Review? review)
    {
        if (review is null) return;
        _chef.ToggleDislike(review);
    }

    [RelayCommand]
    private async Task StartCookingAsync()
    {
        await _navigator.NavigateViewModelAsync<CookingViewModel>(this, data: Recipe);
    }

    [RelayCommand]
    private async Task BackAsync() => await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);
}
