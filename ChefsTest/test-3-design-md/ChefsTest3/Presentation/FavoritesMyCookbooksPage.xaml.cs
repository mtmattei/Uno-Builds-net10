using ChefsTest3.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest3.Presentation;

public sealed partial class FavoritesMyCookbooksPage : Page
{
    public FavoritesMyCookbooksViewModel ViewModel => (FavoritesMyCookbooksViewModel)DataContext;

    public FavoritesMyCookbooksPage()
    {
        this.InitializeComponent();
    }

    private void OnCookbookClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is CookbookData cookbook)
        {
            ViewModel.OpenCookbookCommand.Execute(cookbook);
        }
    }

    private void OnTabSelected(object sender, string tab)
    {
        if (tab == "Home") ViewModel.OpenHomeCommand.Execute(null);
        else if (tab == "Search") ViewModel.OpenSearchCommand.Execute(null);
    }
}
