using Microsoft.UI.Xaml.Controls;

namespace ChefsTest3.Presentation;

public sealed partial class RecipeDetailPage : Page
{
    public RecipeDetailViewModel ViewModel => (RecipeDetailViewModel)DataContext;

    public RecipeDetailPage()
    {
        this.InitializeComponent();
    }
}
