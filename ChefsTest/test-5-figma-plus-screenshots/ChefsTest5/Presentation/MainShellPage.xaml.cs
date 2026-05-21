namespace ChefsTest5.Presentation;

public sealed partial class MainShellPage : Page
{
    public MainShellViewModel ViewModel { get; private set; } = default!;

    public MainShellPage()
    {
        this.InitializeComponent();
        DataContextChanged += (_, __) => { if (DataContext is MainShellViewModel vm) ViewModel = vm; };
    }
}
