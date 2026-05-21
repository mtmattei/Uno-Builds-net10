namespace ChefsTest5.Presentation;

public sealed partial class RecipeDetailPage : Page
{
    public RecipeDetailViewModel ViewModel { get; private set; } = default!;

    public RecipeDetailPage()
    {
        this.InitializeComponent();
        DataContextChanged += (_, __) => { if (DataContext is RecipeDetailViewModel vm) ViewModel = vm; };
    }
}
