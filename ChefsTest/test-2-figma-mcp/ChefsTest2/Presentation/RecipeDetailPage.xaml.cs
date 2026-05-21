using Microsoft.UI.Xaml.Controls;

namespace ChefsTest2.Presentation;

public sealed partial class RecipeDetailPage : Page
{
    public RecipeDetailPage() { this.InitializeComponent(); }
    public RecipeDetailViewModel? ViewModel => DataContext as RecipeDetailViewModel;
}
