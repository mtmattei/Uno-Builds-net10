using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest7.ViewModels;

public partial class OnboardingViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public ObservableCollection<OnboardingFrame> Frames { get; } = new();

    [ObservableProperty]
    private int selectedIndex;

    public OnboardingViewModel(INavigator navigator)
    {
        _navigator = navigator;
        Frames.Add(new OnboardingFrame(
            "ms-appx:///Assets/Welcome/first_splash_screen.png",
            "Discover delicious recipes",
            "Browse a curated catalogue of recipes from chefs around the world."));
        Frames.Add(new OnboardingFrame(
            "ms-appx:///Assets/Welcome/second_splash_screen.png",
            "Cook with confidence",
            "Step-by-step video guides walk you through every recipe."));
        Frames.Add(new OnboardingFrame(
            "ms-appx:///Assets/Welcome/third_splash_screen.png",
            "Save your favorites",
            "Build cookbooks and revisit recipes you love."));
    }

    [RelayCommand]
    private async Task Next()
    {
        if (SelectedIndex < Frames.Count - 1) SelectedIndex++;
        else await _navigator.NavigateRouteAsync(this, "Login");
    }

    [RelayCommand]
    private void Previous()
    {
        if (SelectedIndex > 0) SelectedIndex--;
    }

    [RelayCommand]
    private async Task Skip() => await _navigator.NavigateRouteAsync(this, "Login");
}

public record OnboardingFrame(string ImageUrl, string Title, string Body);
