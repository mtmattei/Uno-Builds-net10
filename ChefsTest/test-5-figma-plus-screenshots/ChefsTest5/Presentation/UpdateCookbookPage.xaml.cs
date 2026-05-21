namespace ChefsTest5.Presentation;

public sealed partial class UpdateCookbookPage : Page
{
    public UpdateCookbookViewModel ViewModel { get; private set; } = default!;

    public UpdateCookbookPage()
    {
        this.InitializeComponent();
        DataContextChanged += (_, __) => { if (DataContext is UpdateCookbookViewModel vm) ViewModel = vm; };
    }
}
