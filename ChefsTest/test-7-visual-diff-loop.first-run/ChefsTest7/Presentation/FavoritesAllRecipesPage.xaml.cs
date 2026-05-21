using ChefsTest7.Models;
using ChefsTest7.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest7.Presentation;

public sealed partial class FavoritesAllRecipesPage : Page
{
    public FavoritesAllRecipesPage() => InitializeComponent();
    private void OnRecipeClick(object sender, ItemClickEventArgs e)
    {
        if (DataContext is FavoritesAllRecipesViewModel vm && e.ClickedItem is RecipeData r)
            vm.OpenRecipeCommand.Execute(r);
    }
}
