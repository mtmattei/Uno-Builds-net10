using System.Collections.ObjectModel;

namespace ChefsTest1.Presentation;

public partial class LiveCookingViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public Recipe Recipe { get; }
    public ObservableCollection<Step> Steps { get; } = new();

    [ObservableProperty] private int currentStepIndex;

    public Step? CurrentStep => CurrentStepIndex >= 0 && CurrentStepIndex < Steps.Count ? Steps[CurrentStepIndex] : null;
    public string AppBarTitle => $"Making {Recipe?.Name}";
    public bool IsFirst => CurrentStepIndex == 0;
    public bool IsLast => CurrentStepIndex >= Steps.Count - 1;

    public LiveCookingViewModel(INavigator navigator, Recipe recipe)
    {
        _navigator = navigator;
        Recipe = recipe;
        if (recipe?.Steps != null) foreach (var s in recipe.Steps) Steps.Add(s);
        BackCommand = new AsyncRelayCommand(BackAsync);
        PreviousCommand = new RelayCommand(Previous);
        NextCommand = new AsyncRelayCommand(NextAsync);
    }

    public ICommand BackCommand { get; }
    public ICommand PreviousCommand { get; }
    public ICommand NextCommand { get; }

    partial void OnCurrentStepIndexChanged(int value)
    {
        OnPropertyChanged(nameof(CurrentStep));
        OnPropertyChanged(nameof(IsFirst));
        OnPropertyChanged(nameof(IsLast));
    }

    private void Previous()
    {
        if (CurrentStepIndex > 0) CurrentStepIndex--;
    }

    private async Task NextAsync()
    {
        if (CurrentStepIndex < Steps.Count - 1) CurrentStepIndex++;
        else await _navigator.NavigateRouteAsync(this, "LiveCookingFinish", data: Recipe);
    }

    private async Task BackAsync()
    {
        await _navigator.NavigateBackAsync(this);
    }
}
