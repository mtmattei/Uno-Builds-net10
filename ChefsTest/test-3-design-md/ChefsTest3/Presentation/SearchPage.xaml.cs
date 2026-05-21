using ChefsTest3.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest3.Presentation;

public sealed partial class SearchPage : Page
{
    public SearchViewModel ViewModel => (SearchViewModel)DataContext;

    public SearchPage()
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
        else if (tab == "Favorites") ViewModel.OpenFavoritesCommand.Execute(null);
    }
}
