namespace ChefsTest5.Presentation;

public sealed partial class LiveCookingPage : Page
{
    public LiveCookingViewModel ViewModel { get; private set; } = default!;

    public LiveCookingPage()
    {
        this.InitializeComponent();
        DataContextChanged += (_, __) => { if (DataContext is LiveCookingViewModel vm) ViewModel = vm; };
    }
}
