using ChefsTest7.Models;
using ChefsTest7.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest7.Presentation;

public sealed partial class FavoritesMyCookbooksPage : Page
{
    public FavoritesMyCookbooksPage() => InitializeComponent();
    private void OnCookbookClick(object sender, ItemClickEventArgs e)
    {
        if (DataContext is FavoritesMyCookbooksViewModel vm && e.ClickedItem is CookbookData c)
            vm.OpenCookbookCommand.Execute(c);
    }
}
