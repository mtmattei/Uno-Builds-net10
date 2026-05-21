using System.Collections.ObjectModel;

namespace ChefsTest1.Presentation;

public class OnboardingFrame
{
    public string Image { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public partial class OnboardingViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public ObservableCollection<OnboardingFrame> Frames { get; } = new()
    {
        new OnboardingFrame
        {
            Image = "ms-appx:///Assets/Welcome/first_splash_screen.png",
            Title = "Welcome to Your App!",
            Body = "Embark on a delightful coding journey as you discover, create, and share awesome script tailored to your app and project preferences."
        },
        new OnboardingFrame
        {
            Image = "ms-appx:///Assets/Welcome/second_splash_screen.png",
            Title = "Browse Delicious Recipes",
            Body = "Find recipes by category, cooking time, or skill level — all backed by chefs from around the world."
        },
        new OnboardingFrame
        {
            Image = "ms-appx:///Assets/Welcome/third_splash_screen.png",
            Title = "Cook Together",
            Body = "Step-by-step instructions, integrated timers, and your own cookbooks help you cook with confidence."
        },
    };

    [ObservableProperty]
    private int selectedIndex;

    public OnboardingViewModel(INavigator navigator)
    {
        _navigator = navigator;
        NextCommand = new AsyncRelayCommand(NextAsync);
        PreviousCommand = new RelayCommand(Previous);
        SkipCommand = new AsyncRelayCommand(SkipAsync);
    }

    public ICommand NextCommand { get; }
    public ICommand PreviousCommand { get; }
    public ICommand SkipCommand { get; }

    private async Task NextAsync()
    {
        if (SelectedIndex < Frames.Count - 1)
        {
            SelectedIndex++;
        }
        else
        {
            await _navigator.NavigateRouteAsync(this, "-/Login");
        }
    }

    private void Previous()
    {
        if (SelectedIndex > 0) SelectedIndex--;
    }

    private async Task SkipAsync()
    {
        await _navigator.NavigateRouteAsync(this, "-/Login");
    }
}
