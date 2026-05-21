namespace ChefsTest5.Presentation;

public sealed partial class NearMeMapPage : Page
{
    public NearMeMapViewModel ViewModel { get; private set; } = default!;

    public NearMeMapPage()
    {
        this.InitializeComponent();
        DataContextChanged += (_, __) => { if (DataContext is NearMeMapViewModel vm) ViewModel = vm; };
    }
}
