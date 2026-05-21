namespace ChefsTest7.Presentation.Pages;

public sealed partial class RecipeDetailPage : Microsoft.UI.Xaml.Controls.Page
{
    public RecipeDetailViewModel? ViewModel => DataContext as RecipeDetailViewModel;
    public RecipeDetailPage() => this.InitializeComponent();
}
