namespace ChefsTest5.Presentation;

public sealed partial class CookbookDetailPage : Page
{
    public CookbookDetailViewModel ViewModel { get; private set; } = default!;

    public CookbookDetailPage()
    {
        this.InitializeComponent();
        DataContextChanged += (_, __) => { if (DataContext is CookbookDetailViewModel vm) ViewModel = vm; };
    }
}
