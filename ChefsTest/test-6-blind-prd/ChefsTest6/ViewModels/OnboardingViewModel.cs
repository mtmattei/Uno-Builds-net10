namespace ChefsTest6.ViewModels;

public partial class OnboardingViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public IReadOnlyList<OnboardingFrame> Frames { get; } = new List<OnboardingFrame>
    {
        new("ms-appx:///Assets/Welcome/first_splash_screen.png",
            "Discover delicious recipes",
            "Browse trending recipes and cuisines hand-picked for you."),
        new("ms-appx:///Assets/Welcome/second_splash_screen.png",
            "Cook hands-free, step by step",
            "Live cooking guides you through each step with embedded video."),
        new("ms-appx:///Assets/Welcome/third_splash_screen.png",
            "Save and organize",
            "Build your own cookbooks and follow other home cooks you love."),
    };

    [ObservableProperty]
    private int currentIndex;

    public OnboardingViewModel(INavigator navigator)
    {
        _navigator = navigator;
    }

    public bool IsFirstFrame => CurrentIndex == 0;
    public bool IsLastFrame => CurrentIndex >= Frames.Count - 1;
    public string PrimaryActionLabel => IsLastFrame ? "Get started" : "Next";

    partial void OnCurrentIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsFirstFrame));
        OnPropertyChanged(nameof(IsLastFrame));
        OnPropertyChanged(nameof(PrimaryActionLabel));
    }

    [RelayCommand]
    private void Previous()
    {
        if (CurrentIndex > 0) CurrentIndex--;
    }

    [RelayCommand]
    private async Task Next()
    {
        if (IsLastFrame)
        {
            await _navigator.NavigateViewModelAsync<LoginViewModel>(this, qualifier: Qualifiers.ClearBackStack);
        }
        else
        {
            CurrentIndex++;
        }
    }

    [RelayCommand]
    private async Task Skip()
    {
        await _navigator.NavigateViewModelAsync<LoginViewModel>(this, qualifier: Qualifiers.ClearBackStack);
    }
}

public record OnboardingFrame(string ImageUrl, string Title, string Body);
