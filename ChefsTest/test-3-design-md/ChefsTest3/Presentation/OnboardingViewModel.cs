using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest3.Presentation;

public partial class OnboardingViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    public IReadOnlyList<OnboardingFrame> Frames { get; } = new List<OnboardingFrame>
    {
        new("ms-appx:///Assets/Welcome/first_splash_screen.png", "Discover delicious recipes",
            "Browse trending dishes and find inspiration for your next meal."),
        new("ms-appx:///Assets/Welcome/second_splash_screen.png", "Save your favorites",
            "Build personal cookbooks of the recipes you love and want to try again."),
        new("ms-appx:///Assets/Welcome/third_splash_screen.png", "Cook step by step",
            "Follow guided cooking with timers, ingredient checklists, and chef-shared videos."),
    };

    [ObservableProperty]
    private int _selectedIndex;

    public bool IsLastPage => SelectedIndex >= Frames.Count - 1;

    partial void OnSelectedIndexChanged(int value) => OnPropertyChanged(nameof(IsLastPage));

    public OnboardingViewModel(INavigator navigator)
    {
        _navigator = navigator;
    }

    [RelayCommand]
    private void Next()
    {
        if (SelectedIndex < Frames.Count - 1)
        {
            SelectedIndex++;
        }
        else
        {
            _ = _navigator.NavigateRouteAsync(this, "Login", qualifier: Qualifiers.ClearBackStack);
        }
    }

    [RelayCommand]
    private void Previous()
    {
        if (SelectedIndex > 0)
        {
            SelectedIndex--;
        }
    }

    [RelayCommand]
    private async void Skip()
    {
        await _navigator.NavigateRouteAsync(this, "Login", qualifier: Qualifiers.ClearBackStack);
    }
}

public record OnboardingFrame(string ImageUrl, string Title, string Description);
