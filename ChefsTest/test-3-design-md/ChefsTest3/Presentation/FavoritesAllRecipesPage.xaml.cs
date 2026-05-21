using ChefsTest3.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest3.Presentation;

public sealed partial class FavoritesAllRecipesPage : Page
{
    public FavoritesAllRecipesViewModel ViewModel => (FavoritesAllRecipesViewModel)DataContext;

    public FavoritesAllRecipesPage()
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

    private void OnTabSelected(object sender, string tab)
    {
        if (tab == "Home") ViewModel.OpenHomeCommand.Execute(null);
        else if (tab == "Search") ViewModel.OpenSearchCommand.Execute(null);
    }
}
