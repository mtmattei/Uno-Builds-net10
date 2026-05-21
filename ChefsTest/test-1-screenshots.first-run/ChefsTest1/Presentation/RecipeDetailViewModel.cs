using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChefsTest1.Models;
using ChefsTest1.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class RecipeDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private RecipeData? recipe;
    [ObservableProperty] private int activeTab;
    [ObservableProperty] private ObservableCollection<IngredientData> ingredients = new();
    [ObservableProperty] private ObservableCollection<StepData> steps = new();
    [ObservableProperty] private ObservableCollection<ReviewData> reviews = new();
    [ObservableProperty] private bool isIngredientsTab = true;
    [ObservableProperty] private bool isStepsTab;
    [ObservableProperty] private bool isReviewsTab;
    [ObservableProperty] private bool isNutritionTab;
    [ObservableProperty] private bool hasReviews;

    public RecipeDetailViewModel(INavigator navigator, IChefsDataService data, RecipeData? recipe = null)
    {
        _navigator = navigator;
        _data = data;
        if (recipe != null) Bind(recipe);
        SelectIngredients();
    }

    public void Bind(RecipeData? r)
    {
        Recipe = r;
        if (r != null)
        {
            Ingredients = new ObservableCollection<IngredientData>(r.Ingredients ?? new());
            Steps = new ObservableCollection<StepData>(r.Steps ?? new());
            Reviews = new ObservableCollection<ReviewData>(r.Reviews ?? new());
            HasReviews = (r.Reviews?.Count ?? 0) > 0;
        }
    }

    [RelayCommand] private void SelectIngredients() => SetTab(0);
    [RelayCommand] private void SelectSteps() => SetTab(1);
    [RelayCommand] private void SelectReviews() => SetTab(2);
    [RelayCommand] private void SelectNutrition() => SetTab(3);

    private void SetTab(int tab)
    {
        ActiveTab = tab;
        IsIngredientsTab = tab == 0;
        IsStepsTab = tab == 1;
        IsReviewsTab = tab == 2;
        IsNutritionTab = tab == 3;
    }

    [RelayCommand]
    private Task StartCooking() =>
        Recipe == null ? Task.CompletedTask : _navigator.NavigateViewModelAsync<LiveCookingViewModel>(this, data: Recipe);

    [RelayCommand]
    private Task Back() => _navigator.NavigateBackAsync(this);

    [RelayCommand]
    private async Task ToggleFav()
    {
        if (Recipe != null)
        {
            await _data.ToggleFavoriteAsync(Recipe.Id);
            OnPropertyChanged(nameof(Recipe));
        }
    }
}
