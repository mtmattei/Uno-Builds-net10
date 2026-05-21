namespace ChefsTest6.ViewModels;

public partial class LiveCookingViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public RecipeData Recipe { get; }
    public IReadOnlyList<StepData> Steps { get; }

    [ObservableProperty]
    private int currentStepIndex;

    public LiveCookingViewModel(INavigator navigator, RecipeData recipe)
    {
        _navigator = navigator;
        Recipe = recipe;
        Steps = recipe.Steps ?? new List<StepData>();
    }

    public StepData? CurrentStep => Steps.Count == 0 ? null : Steps[Math.Clamp(CurrentStepIndex, 0, Steps.Count - 1)];
    public int StepNumber => CurrentStepIndex + 1;
    public int StepTotal => Steps.Count;
    public string ProgressLabel => $"Step {StepNumber} of {StepTotal}";
    public bool IsFirst => CurrentStepIndex == 0;
    public bool IsLast => CurrentStepIndex >= Steps.Count - 1;
    public string PrimaryActionLabel => IsLast ? "Done" : "Next";

    public string AppBarTitle => $"Making {Recipe.Name}";

    partial void OnCurrentStepIndexChanged(int value)
    {
        OnPropertyChanged(nameof(CurrentStep));
        OnPropertyChanged(nameof(StepNumber));
        OnPropertyChanged(nameof(ProgressLabel));
        OnPropertyChanged(nameof(IsFirst));
        OnPropertyChanged(nameof(IsLast));
        OnPropertyChanged(nameof(PrimaryActionLabel));
    }

    [RelayCommand]
    private async Task Next()
    {
        if (IsLast)
        {
            await _navigator.NavigateViewModelAsync<LiveCookingFinishViewModel>(this, data: Recipe);
        }
        else
        {
            CurrentStepIndex++;
        }
    }

    [RelayCommand]
    private void Previous()
    {
        if (CurrentStepIndex > 0) CurrentStepIndex--;
    }
}
