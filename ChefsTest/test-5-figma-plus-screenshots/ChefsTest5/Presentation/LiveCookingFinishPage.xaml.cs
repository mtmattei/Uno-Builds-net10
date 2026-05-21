namespace ChefsTest5.Presentation;

public sealed partial class LiveCookingFinishPage : Page
{
    public LiveCookingFinishViewModel ViewModel { get; private set; } = default!;

    public LiveCookingFinishPage()
    {
        this.InitializeComponent();
        DataContextChanged += (_, __) => { if (DataContext is LiveCookingFinishViewModel vm) ViewModel = vm; };
    }
}
