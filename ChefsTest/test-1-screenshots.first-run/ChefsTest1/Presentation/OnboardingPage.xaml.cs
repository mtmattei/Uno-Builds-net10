using Microsoft.UI.Xaml.Controls;

namespace ChefsTest1.Presentation;

public sealed partial class OnboardingPage : Page
{
    public OnboardingViewModel ViewModel => (OnboardingViewModel)DataContext;

    public OnboardingPage()
    {
        this.InitializeComponent();
    }
}
