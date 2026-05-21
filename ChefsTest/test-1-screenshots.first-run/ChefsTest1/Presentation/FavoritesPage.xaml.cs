using ChefsTest1.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest1.Presentation;

public sealed partial class FavoritesPage : Page
{
    public FavoritesViewModel ViewModel => (FavoritesViewModel)DataContext;

    public FavoritesPage()
    {
        this.InitializeComponent();
    }

    private void OnRecipeCardClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is RecipeData r)
            ViewModel?.OpenRecipeCommand.Execute(r);
    }

    private void OnCookbookCardClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is CookbookData c)
            ViewModel?.OpenCookbookCommand.Execute(c);
    }
}
