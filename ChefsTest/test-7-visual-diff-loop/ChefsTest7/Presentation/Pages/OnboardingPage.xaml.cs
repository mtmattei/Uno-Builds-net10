namespace ChefsTest7.Presentation.Pages;

public sealed partial class OnboardingPage : Microsoft.UI.Xaml.Controls.Page
{
    public OnboardingViewModel? ViewModel => DataContext as OnboardingViewModel;
    public OnboardingPage() => this.InitializeComponent();
}
