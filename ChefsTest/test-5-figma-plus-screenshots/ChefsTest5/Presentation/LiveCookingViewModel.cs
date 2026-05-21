using System.Collections.ObjectModel;

namespace ChefsTest5.Presentation;

public partial class LiveCookingViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty] private RecipeData recipe = new();
    [ObservableProperty] private int stepIndex;

    public StepData? CurrentStep => Recipe.Steps is not null && Recipe.Steps.Count > 0 && StepIndex < Recipe.Steps.Count
        ? Recipe.Steps[StepIndex] : null;

    public string CurrentStepHeading => CurrentStep is null ? string.Empty : $"{CurrentStep.Number}- {CurrentStep.Name}";
    public IReadOnlyList<string> CurrentIngredients => CurrentStep?.Ingredients ?? new List<string>();
    public string CurrentDescription => CurrentStep?.Description ?? string.Empty;
    public IReadOnlyList<bool> PipDots => Enumerable.Range(0, Recipe.Steps?.Count ?? 0).Select(i => i == StepIndex).ToList();
    public string NextLabel => CurrentStep is null || (Recipe.Steps is not null && StepIndex >= Recipe.Steps.Count - 1) ? "Finish" : "Next";

    public IRelayCommand PreviousCommand { get; }
    public IAsyncRelayCommand NextCommand { get; }
    public IAsyncRelayCommand BackCommand { get; }

    public LiveCookingViewModel(INavigator navigator, RecipeData? data = null)
    {
        _navigator = navigator;
        if (data is not null) Recipe = data;
        PreviousCommand = new RelayCommand(() => { if (StepIndex > 0) StepIndex--; });
        NextCommand = new AsyncRelayCommand(async () =>
        {
            if (Recipe.Steps is null || StepIndex >= Recipe.Steps.Count - 1)
            {
                await _navigator.NavigateViewModelAsync<LiveCookingFinishViewModel>(this, data: Recipe);
                return;
            }
            StepIndex++;
        });
        BackCommand = new AsyncRelayCommand(async () => await _navigator.NavigateBackAsync(this));
    }

    partial void OnStepIndexChanged(int value)
    {
        OnPropertyChanged(nameof(CurrentStep));
        OnPropertyChanged(nameof(CurrentStepHeading));
        OnPropertyChanged(nameof(CurrentIngredients));
        OnPropertyChanged(nameof(CurrentDescription));
        OnPropertyChanged(nameof(PipDots));
        OnPropertyChanged(nameof(NextLabel));
    }

    partial void OnRecipeChanged(RecipeData value)
    {
        OnPropertyChanged(nameof(CurrentStep));
        OnPropertyChanged(nameof(CurrentStepHeading));
        OnPropertyChanged(nameof(CurrentIngredients));
        OnPropertyChanged(nameof(CurrentDescription));
        OnPropertyChanged(nameof(PipDots));
        OnPropertyChanged(nameof(NextLabel));
    }
}
