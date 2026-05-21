using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChefsTest7.Models;
using ChefsTest7.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest7.ViewModels;

public partial class RecipeDetailViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private RecipeData? recipe;
    [ObservableProperty] private int selectedTab; // 0:Ingredients 1:Steps 2:Reviews 3:Nutrition

    public ObservableCollection<IngredientData> Ingredients { get; } = new();
    public ObservableCollection<StepData> Steps { get; } = new();
    public ObservableCollection<ReviewData> Reviews { get; } = new();

    public RecipeDetailViewModel(INavigator navigator, IChefsDataService data, RecipeData? recipe = null)
    {
        _navigator = navigator; _data = data;
        if (recipe is not null)
        {
            Recipe = recipe;
            foreach (var i in recipe.Ingredients ?? Enumerable.Empty<IngredientData>()) Ingredients.Add(i);
            foreach (var s in recipe.Steps ?? Enumerable.Empty<StepData>()) Steps.Add(s);
            foreach (var rv in recipe.Reviews ?? Enumerable.Empty<ReviewData>()) Reviews.Add(rv);
        }
        else _ = LoadFirstAsync();
    }

    private async Task LoadFirstAsync()
    {
        var recipes = await _data.GetRecipesAsync();
        if (recipes.Count == 0) return;
        Recipe = recipes[0];
        foreach (var i in Recipe.Ingredients ?? Enumerable.Empty<IngredientData>()) Ingredients.Add(i);
        foreach (var s in Recipe.Steps ?? Enumerable.Empty<StepData>()) Steps.Add(s);
        foreach (var rv in Recipe.Reviews ?? Enumerable.Empty<ReviewData>()) Reviews.Add(rv);
    }

    [RelayCommand]
    private async Task StartCooking()
    {
        if (Recipe is null) return;
        await _navigator.NavigateRouteAsync(this, "LiveCooking", data: Recipe);
    }

    [RelayCommand]
    private async Task ToggleFavorite()
    {
        if (Recipe is null) return;
        await _data.ToggleFavoriteAsync(Recipe.Id);
        Recipe.IsFavorite = !Recipe.IsFavorite;
        OnPropertyChanged(nameof(Recipe));
    }

    [RelayCommand]
    private async Task Back() => await _navigator.NavigateBackAsync(this);
}

public partial class LiveCookingViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    [ObservableProperty] private RecipeData? recipe;
    [ObservableProperty] private int currentStepIndex;
    public ObservableCollection<StepData> Steps { get; } = new();
    public StepData? CurrentStep => CurrentStepIndex >= 0 && CurrentStepIndex < Steps.Count ? Steps[CurrentStepIndex] : null;

    public LiveCookingViewModel(INavigator navigator, RecipeData? recipe = null)
    {
        _navigator = navigator;
        Recipe = recipe;
        foreach (var s in recipe?.Steps ?? Enumerable.Empty<StepData>()) Steps.Add(s);
    }

    partial void OnCurrentStepIndexChanged(int value) { OnPropertyChanged(nameof(CurrentStep)); }

    [RelayCommand]
    private async Task Next()
    {
        if (CurrentStepIndex < Steps.Count - 1) CurrentStepIndex++;
        else await _navigator.NavigateRouteAsync(this, "LiveCookingFinish", data: Recipe);
    }

    [RelayCommand]
    private void Previous() { if (CurrentStepIndex > 0) CurrentStepIndex--; }

    [RelayCommand]
    private async Task Back() => await _navigator.NavigateBackAsync(this);
}

public partial class LiveCookingFinishViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    [ObservableProperty] private RecipeData? recipe;
    [ObservableProperty] private int rating = 5;

    public LiveCookingFinishViewModel(INavigator navigator, RecipeData? recipe = null)
    { _navigator = navigator; Recipe = recipe; }

    [RelayCommand] private async Task Previous() => await _navigator.NavigateBackAsync(this);
    [RelayCommand] private async Task Favorite() => await _navigator.NavigateRouteAsync(this, "Main");
    [RelayCommand] private void SetRating(int r) => Rating = r;
}
