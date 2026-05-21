using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChefsTest1.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class LiveCookingViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty] private RecipeData? recipe;
    [ObservableProperty] private int stepIndex;
    [ObservableProperty] private StepData? currentStep;
    [ObservableProperty] private string stepHeading = "1- Step";
    [ObservableProperty] private int totalSteps;
    [ObservableProperty] private bool isLastStep;

    public LiveCookingViewModel(INavigator navigator, RecipeData? recipe = null)
    {
        _navigator = navigator;
        if (recipe != null) Bind(recipe);
    }

    public void Bind(RecipeData? r)
    {
        Recipe = r;
        TotalSteps = r?.Steps?.Count ?? 0;
        UpdateStep();
    }

    partial void OnStepIndexChanged(int value) => UpdateStep();

    private void UpdateStep()
    {
        if (Recipe?.Steps != null && Recipe.Steps.Count > 0)
        {
            var clamped = Math.Clamp(StepIndex, 0, Recipe.Steps.Count - 1);
            CurrentStep = Recipe.Steps[clamped];
            StepHeading = $"{clamped + 1}- {CurrentStep.Name}";
            IsLastStep = clamped == Recipe.Steps.Count - 1;
        }
    }

    [RelayCommand]
    private async Task Next()
    {
        if (Recipe?.Steps == null || Recipe.Steps.Count == 0) return;
        if (StepIndex < Recipe.Steps.Count - 1)
            StepIndex++;
        else
            await _navigator.NavigateViewModelAsync<LiveCookingFinishViewModel>(this);
    }

    [RelayCommand]
    private void Previous()
    {
        if (StepIndex > 0) StepIndex--;
    }

    [RelayCommand]
    private Task Back() => _navigator.NavigateBackAsync(this);
}
