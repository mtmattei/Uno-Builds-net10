namespace ChefsTest1.Presentation;

public sealed partial class OnboardingPage : Page
{
    public OnboardingPage()
    {
        this.InitializeComponent();
    }

    public OnboardingViewModel? ViewModel => DataContext as OnboardingViewModel;
}
