namespace ChefsTest1.Presentation;

public sealed partial class RecipeDetailPage : Page
{
    public RecipeDetailPage()
    {
        this.InitializeComponent();
    }

    public RecipeDetailViewModel? ViewModel => DataContext as RecipeDetailViewModel;
}
