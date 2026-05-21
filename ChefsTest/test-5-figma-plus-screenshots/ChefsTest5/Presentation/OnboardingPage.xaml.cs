namespace ChefsTest5.Presentation;

public sealed partial class OnboardingPage : Page
{
    public OnboardingViewModel ViewModel { get; private set; } = default!;

    public OnboardingPage()
    {
        this.InitializeComponent();
        DataContextChanged += (_, __) =>
        {
            if (DataContext is OnboardingViewModel vm) ViewModel = vm;
        };
    }
}
