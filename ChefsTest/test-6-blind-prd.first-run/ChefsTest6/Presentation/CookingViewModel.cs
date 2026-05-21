using System.Collections.ObjectModel;

namespace ChefsTest6.Presentation;

public partial class CookingViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public Recipe Recipe { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StepNumberText), nameof(CurrentStep), nameof(StepTitle),
        nameof(StepDescription), nameof(IsLastStep), nameof(IsFirstStep),
        nameof(NextLabel), nameof(VideoSource), nameof(StepIngredients), nameof(StepCounterText))]
    private int currentIndex;

    public CookingStep? CurrentStep =>
        Recipe.Steps.Count > CurrentIndex ? Recipe.Steps[CurrentIndex] : null;

    public string StepNumberText => CurrentStep is null
        ? string.Empty
        : $"{CurrentStep.Number}";

    public string StepTitle => CurrentStep?.Name ?? string.Empty;
    public string StepDescription => CurrentStep?.Description ?? string.Empty;

    public IEnumerable<string> StepIngredients => CurrentStep?.Ingredients ?? Array.Empty<string>();

    public bool IsFirstStep => CurrentIndex == 0;
    public bool IsLastStep => CurrentIndex >= Recipe.Steps.Count - 1;
    public string NextLabel => IsLastStep ? "Done" : "Next";

    public string StepCounterText => $"Step {CurrentIndex + 1} of {Recipe.Steps.Count}";

    public Uri? VideoSource =>
        Uri.TryCreate(CurrentStep?.UrlVideo, UriKind.Absolute, out var uri) ? uri : null;

    public ObservableCollection<int> StepIndicators { get; } = new();

    public CookingViewModel(INavigator navigator, Recipe recipe)
    {
        _navigator = navigator;
        Recipe = recipe;
        for (var i = 0; i < recipe.Steps.Count; i++)
        {
            StepIndicators.Add(i);
        }
    }

    [RelayCommand]
    private async Task NextAsync()
    {
        if (IsLastStep)
        {
            await _navigator.NavigateViewModelAsync<CookingDoneViewModel>(this, data: Recipe,
                qualifier: Qualifiers.ClearBackStack);
            return;
        }
        CurrentIndex++;
    }

    [RelayCommand]
    private void Previous()
    {
        if (CurrentIndex > 0) CurrentIndex--;
    }

    [RelayCommand]
    private async Task BackAsync() => await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);
}
