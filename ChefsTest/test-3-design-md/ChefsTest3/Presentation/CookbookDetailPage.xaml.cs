using ChefsTest3.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest3.Presentation;

public sealed partial class CookbookDetailPage : Page
{
    public CookbookDetailViewModel ViewModel => (CookbookDetailViewModel)DataContext;

    public CookbookDetailPage()
    {
        this.InitializeComponent();
    }

    private void OnRecipeClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is RecipeData recipe)
        {
            ViewModel.OpenRecipeCommand.Execute(recipe);
        }
    }
}
