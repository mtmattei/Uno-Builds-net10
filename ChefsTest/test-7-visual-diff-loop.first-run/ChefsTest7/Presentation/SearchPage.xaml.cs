using ChefsTest7.Models;
using ChefsTest7.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest7.Presentation;

public sealed partial class SearchPage : Page
{
    public SearchPage() => InitializeComponent();
    private void OnRecipeClick(object sender, ItemClickEventArgs e)
    {
        if (DataContext is SearchViewModel vm && e.ClickedItem is RecipeData r)
            vm.OpenRecipeCommand.Execute(r);
    }
}
