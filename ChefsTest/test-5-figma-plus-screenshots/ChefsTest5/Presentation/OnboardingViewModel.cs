using System.Collections.ObjectModel;

namespace ChefsTest5.Presentation;

public sealed record OnboardingFrame(string ImageUri, string Title, string Body);

public partial class OnboardingViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public ObservableCollection<OnboardingFrame> Frames { get; } = new()
    {
        new("ms-appx:///Assets/Welcome/first_splash_screen.png", "Welcome to Your App!",
            "Embark on a delightful coding journey as you discover, create, and share awesome script tailored to your app and project preferences."),
        new("ms-appx:///Assets/Welcome/second_splash_screen.png", "Discover Recipes",
            "Browse trending dishes, save your favorites, and explore tastes from around the world — all in one place."),
        new("ms-appx:///Assets/Welcome/third_splash_screen.png", "Cook With Confidence",
            "Step-by-step videos, ingredient checklists, and a finish-line celebration when your meal is ready.")
    };

    [ObservableProperty]
    private int selectedIndex;

    public string NextLabel => SelectedIndex < Frames.Count - 1 ? "Next" : "Get started";

    partial void OnSelectedIndexChanged(int value) => OnPropertyChanged(nameof(NextLabel));

    public IRelayCommand NextCommand { get; }
    public IRelayCommand PreviousCommand { get; }
    public IAsyncRelayCommand SkipCommand { get; }

    public OnboardingViewModel(INavigator navigator)
    {
        _navigator = navigator;
        NextCommand = new RelayCommand(async () =>
        {
            if (SelectedIndex < Frames.Count - 1) SelectedIndex++;
            else await _navigator.NavigateViewModelAsync<LoginViewModel>(this, qualifier: Qualifiers.ClearBackStack);
        });
        PreviousCommand = new RelayCommand(() =>
        {
            if (SelectedIndex > 0) SelectedIndex--;
        });
        SkipCommand = new AsyncRelayCommand(async () =>
            await _navigator.NavigateViewModelAsync<LoginViewModel>(this, qualifier: Qualifiers.ClearBackStack));
    }
}
