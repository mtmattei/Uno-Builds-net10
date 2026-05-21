using System.Collections.ObjectModel;

namespace ChefsTest1.Presentation;

public partial class RecipeDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public Recipe Recipe { get; }
    public ObservableCollection<Ingredient> Ingredients { get; } = new();
    public ObservableCollection<Step> Steps { get; } = new();
    public ObservableCollection<Review> Reviews { get; } = new();
    public Nutrition? Nutrition { get; }

    [ObservableProperty] private int selectedTabIndex; // 0..3

    public bool IsIngredients => SelectedTabIndex == 0;
    public bool IsSteps => SelectedTabIndex == 1;
    public bool IsReviews => SelectedTabIndex == 2;
    public bool IsNutrition => SelectedTabIndex == 3;

    public bool HasReviews => Reviews.Count > 0;
    public bool HasNoReviews => Reviews.Count == 0;
    public string IngredientsLabel => $"{Ingredients.Count} items";
    public string StepsLabel => $"{Steps.Count} steps";
    public string ReviewsLabel => Reviews.Count == 1 ? "1 comment" : $"{Reviews.Count} comments";
    public string CookTimeLabel => Recipe?.CookTimeMinutes ?? string.Empty;
    public string DifficultyLabel => Recipe?.DifficultyLabel ?? string.Empty;
    public string CaloriesLabel => Recipe?.Calories ?? string.Empty;
    public string CreatorByline => $"By {Recipe?.Creator?.FullName}";

    public RecipeDetailViewModel(INavigator navigator, Recipe recipe)
    {
        _navigator = navigator;
        Recipe = recipe;
        if (recipe?.Ingredients != null) foreach (var i in recipe.Ingredients) Ingredients.Add(i);
        if (recipe?.Steps != null) foreach (var s in recipe.Steps) Steps.Add(s);
        if (recipe?.Reviews != null) foreach (var r in recipe.Reviews) Reviews.Add(r);
        Nutrition = recipe?.Nutrition ?? new Nutrition { Protein = 30, ProteinBase = 110, Carbs = 101, CarbsBase = 300, Fat = 30, FatBase = 75 };

        SelectIngredientsCommand = new RelayCommand(() => SelectedTabIndex = 0);
        SelectStepsCommand = new RelayCommand(() => SelectedTabIndex = 1);
        SelectReviewsCommand = new RelayCommand(() => SelectedTabIndex = 2);
        SelectNutritionCommand = new RelayCommand(() => SelectedTabIndex = 3);
        StartCookingCommand = new AsyncRelayCommand(StartCookingAsync);
        BackCommand = new AsyncRelayCommand(BackAsync);
        AddCommentCommand = new RelayCommand(() => { });
    }

    public ICommand SelectIngredientsCommand { get; }
    public ICommand SelectStepsCommand { get; }
    public ICommand SelectReviewsCommand { get; }
    public ICommand SelectNutritionCommand { get; }
    public ICommand StartCookingCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand AddCommentCommand { get; }

    partial void OnSelectedTabIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsIngredients));
        OnPropertyChanged(nameof(IsSteps));
        OnPropertyChanged(nameof(IsReviews));
        OnPropertyChanged(nameof(IsNutrition));
    }

    private async Task StartCookingAsync()
    {
        await _navigator.NavigateRouteAsync(this, "LiveCooking", data: Recipe);
    }

    private async Task BackAsync()
    {
        await _navigator.NavigateBackAsync(this);
    }
}
