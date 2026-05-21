using System.Collections.ObjectModel;
using System.Linq;
using ChefsTest3.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest3.Presentation;

public partial class LiveCookingViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty]
    private RecipeData _recipe;

    [ObservableProperty]
    private int _currentStepIndex;

    public ObservableCollection<StepData> Steps { get; } = new();

    public StepData? CurrentStep => Steps.Count > 0 ? Steps[CurrentStepIndex] : null;
    public string StepLabel => $"Step {CurrentStepIndex + 1} of {Steps.Count}";
    public bool CanGoPrevious => CurrentStepIndex > 0;
    public bool CanGoNext => CurrentStepIndex < Steps.Count - 1;
    public bool IsLastStep => Steps.Count > 0 && CurrentStepIndex == Steps.Count - 1;

    public LiveCookingViewModel(INavigator navigator, RecipeData recipe)
    {
        _navigator = navigator;
        _recipe = recipe ?? new RecipeData { Name = "Recipe" };
        if (_recipe.Steps is not null)
        {
            foreach (var s in _recipe.Steps) Steps.Add(s);
        }
    }

    partial void OnCurrentStepIndexChanged(int value)
    {
        OnPropertyChanged(nameof(CurrentStep));
        OnPropertyChanged(nameof(StepLabel));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(IsLastStep));
    }

    [RelayCommand]
    private void Previous()
    {
        if (CanGoPrevious) CurrentStepIndex--;
    }

    [RelayCommand]
    private async void Next()
    {
        if (CanGoNext)
        {
            CurrentStepIndex++;
        }
        else
        {
            await _navigator.NavigateRouteAsync(this, "LiveCookingFinish");
        }
    }

    [RelayCommand]
    private async void GoBack()
    {
        await _navigator.NavigateBackAsync(this);
    }
}
