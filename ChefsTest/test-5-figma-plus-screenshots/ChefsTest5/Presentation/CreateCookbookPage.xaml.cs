namespace ChefsTest5.Presentation;

public sealed partial class CreateCookbookPage : Page
{
    public CreateCookbookViewModel ViewModel { get; private set; } = default!;

    public CreateCookbookPage()
    {
        this.InitializeComponent();
        DataContextChanged += (_, __) => { if (DataContext is CreateCookbookViewModel vm) ViewModel = vm; };
    }
}
