using Microsoft.UI.Xaml.Controls;

namespace ChefsTest3.Presentation;

public sealed partial class OnboardingPage : Page
{
    public OnboardingViewModel ViewModel => (OnboardingViewModel)DataContext;

    public OnboardingPage()
    {
        this.InitializeComponent();
    }
}
