using System.Collections.ObjectModel;

namespace ChefsTest6.Presentation;

public partial class OnboardingViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public ObservableCollection<OnboardingSlide> Slides { get; } = new()
    {
        new("Discover delicious recipes",
            "Find inspiring recipes across cuisines, skill levels, and time budgets — all in one place.",
            "ms-appx:///Assets/Welcome/first_splash_screen.png"),
        new("Save and organize favorites",
            "Build your own cookbooks. Tag recipes you love, group them by mood or occasion, and find them again in a tap.",
            "ms-appx:///Assets/Welcome/second_splash_screen.png"),
        new("Cook hands-free",
            "Step-by-step guidance with embedded video. Stay on the right step even when your hands are messy.",
            "ms-appx:///Assets/Welcome/third_splash_screen.png"),
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFirstSlide), nameof(IsLastSlide), nameof(NextLabel))]
    private int currentIndex;

    public bool IsFirstSlide => CurrentIndex <= 0;
    public bool IsLastSlide => CurrentIndex >= Slides.Count - 1;
    public string NextLabel => IsLastSlide ? "Get started" : "Next";

    public OnboardingViewModel(INavigator navigator)
    {
        _navigator = navigator;
    }

    [RelayCommand]
    private void Previous()
    {
        if (CurrentIndex > 0) CurrentIndex--;
    }

    [RelayCommand]
    private async Task NextAsync()
    {
        if (CurrentIndex < Slides.Count - 1)
        {
            CurrentIndex++;
            return;
        }
        await _navigator.NavigateViewModelAsync<LoginViewModel>(this, qualifier: Qualifiers.ClearBackStack);
    }

    [RelayCommand]
    private async Task SkipAsync()
    {
        await _navigator.NavigateViewModelAsync<LoginViewModel>(this, qualifier: Qualifiers.ClearBackStack);
    }
}
