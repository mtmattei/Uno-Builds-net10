using Microsoft.UI.Xaml.Controls;

namespace ChefsTest2.Presentation;

public sealed partial class OnboardingPage : Page
{
    public OnboardingPage()
    {
        this.InitializeComponent();
        this.DataContextChanged += (s, e) => Bindings.Update();
    }

    public OnboardingViewModel? ViewModel => DataContext as OnboardingViewModel;

    private string GetTitle(int idx)
        => ViewModel is { } vm && idx >= 0 && idx < vm.Titles.Length ? vm.Titles[idx] : string.Empty;

    private string GetBody(int idx)
        => ViewModel is { } vm && idx >= 0 && idx < vm.Bodies.Length ? vm.Bodies[idx] : string.Empty;
}
