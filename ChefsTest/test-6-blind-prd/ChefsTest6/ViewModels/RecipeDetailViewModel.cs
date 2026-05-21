namespace ChefsTest6.ViewModels;

public partial class RecipeDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IRecipeService _recipes;

    public RecipeData Recipe { get; }
    public IReadOnlyList<IngredientData> Ingredients => Recipe.Ingredients ?? new List<IngredientData>();
    public IReadOnlyList<StepData> Steps => Recipe.Steps ?? new List<StepData>();
    public IReadOnlyList<ReviewData> Reviews => Recipe.Reviews ?? new List<ReviewData>();
    public NutritionData Nutrition => Recipe.Nutrition ?? new NutritionData
    {
        Protein = 30, ProteinBase = 110,
        Carbs = 80, CarbsBase = 250,
        Fat = 18, FatBase = 70,
    };

    [ObservableProperty]
    private int activeTabIndex;

    public string IngredientCountLabel => $"{Ingredients.Count} items";
    public string StepCountLabel => $"{Steps.Count} steps";
    public string ReviewCountLabel => $"{Reviews.Count} comments";
    public bool HasReviews => Reviews.Count > 0;
    public string CalorieValue
    {
        get
        {
            var s = Recipe.Calories ?? "0";
            var digits = new string(s.Where(char.IsDigit).ToArray());
            return string.IsNullOrEmpty(digits) ? "0" : digits;
        }
    }

    public RecipeDetailViewModel(INavigator navigator, IRecipeService recipes, RecipeData recipe)
    {
        _navigator = navigator;
        _recipes = recipes;
        Recipe = recipe;
    }

    [RelayCommand]
    private async Task StartCooking()
    {
        await _navigator.NavigateViewModelAsync<LiveCookingViewModel>(this, data: Recipe);
    }

    [RelayCommand]
    private void ToggleFavorite() => _recipes.ToggleFavorite(Recipe);

    [RelayCommand]
    private void Share() { /* stub: visible action, no real share sheet */ }

    [RelayCommand]
    private void LikeReview(ReviewData? review)
    {
        if (review is null) return;
        review.IsLiked = !review.IsLiked;
        if (review.IsLiked && review.IsDisliked)
        {
            review.IsDisliked = false;
            review.DislikeCount = Math.Max(0, review.DislikeCount - 1);
        }
        review.LikeCount += review.IsLiked ? 1 : -1;
        if (review.LikeCount < 0) review.LikeCount = 0;
    }

    [RelayCommand]
    private void DislikeReview(ReviewData? review)
    {
        if (review is null) return;
        review.IsDisliked = !review.IsDisliked;
        if (review.IsDisliked && review.IsLiked)
        {
            review.IsLiked = false;
            review.LikeCount = Math.Max(0, review.LikeCount - 1);
        }
        review.DislikeCount += review.IsDisliked ? 1 : -1;
        if (review.DislikeCount < 0) review.DislikeCount = 0;
    }
}
