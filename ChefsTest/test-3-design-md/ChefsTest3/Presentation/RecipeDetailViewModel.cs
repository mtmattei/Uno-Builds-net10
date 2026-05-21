using System;
using System.Collections.ObjectModel;
using System.Linq;
using ChefsTest3.Models;
using ChefsTest3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest3.Presentation;

public partial class RecipeDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsService _service;

    [ObservableProperty]
    private RecipeData _recipe;

    [ObservableProperty]
    private string _activeTab = "Ingredients";

    [ObservableProperty]
    private bool _isFavorite;

    [ObservableProperty]
    private bool _showIngredients = true;

    [ObservableProperty]
    private bool _showSteps;

    [ObservableProperty]
    private bool _showReviews;

    [ObservableProperty]
    private bool _showNutrition;

    public ObservableCollection<IngredientData> Ingredients { get; } = new();
    public ObservableCollection<StepData> Steps { get; } = new();
    public ObservableCollection<ReviewData> Reviews { get; } = new();

    public string CookTimeDisplay => BindableHelpers.FormatCookTime(Recipe?.CookTime ?? TimeSpan.Zero);
    public string DifficultyLabel => BindableHelpers.DifficultyLabel(Recipe?.Difficulty ?? 0);
    public string ServesDisplay => $"{Recipe?.Serves ?? 0} serves";
    public string CaloriesDisplay => Recipe?.Calories ?? "—";
    public string CreatorName => Recipe?.Creator?.FullName ?? "Chef";
    public string CreatorAvatar => Recipe?.Creator?.UrlProfileImage ?? string.Empty;
    public NutritionData Nutrition => Recipe?.Nutrition ?? new NutritionData
    {
        Protein = 25, ProteinBase = 100,
        Carbs = 45, CarbsBase = 100,
        Fat = 30, FatBase = 100,
    };

    public RecipeDetailViewModel(INavigator navigator, IChefsService service, RecipeData recipe)
    {
        _navigator = navigator;
        _service = service;
        _recipe = recipe ?? new RecipeData { Name = "Recipe", Calories = "—" };
        IsFavorite = _recipe.IsFavorite;
        if (_recipe.Ingredients is not null)
        {
            foreach (var i in _recipe.Ingredients) Ingredients.Add(i);
        }
        if (_recipe.Steps is not null)
        {
            foreach (var s in _recipe.Steps) Steps.Add(s);
        }
        if (_recipe.Reviews is not null)
        {
            foreach (var r in _recipe.Reviews) Reviews.Add(r);
        }
    }

    [RelayCommand]
    private void SetTab(string tab)
    {
        ActiveTab = tab;
        ShowIngredients = tab == "Ingredients";
        ShowSteps = tab == "Steps";
        ShowReviews = tab == "Reviews";
        ShowNutrition = tab == "Nutrition";
    }

    [RelayCommand]
    private async void GoBack()
    {
        await _navigator.NavigateBackAsync(this);
    }

    [RelayCommand]
    private async void ToggleFavorite()
    {
        IsFavorite = !IsFavorite;
        Recipe.IsFavorite = IsFavorite;
        await _service.ToggleFavoriteRecipeAsync(Recipe.Id);
    }

    [RelayCommand]
    private async void StartCooking()
    {
        await _navigator.NavigateViewModelAsync<LiveCookingViewModel>(this, data: Recipe);
    }
}
