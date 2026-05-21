using ChefsTest1.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest1.Presentation;

public sealed partial class SearchPage : Page
{
    public SearchViewModel ViewModel => (SearchViewModel)DataContext;

    public SearchPage()
    {
        this.InitializeComponent();
    }

    private void OnRecipeCardClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is RecipeData r)
            ViewModel?.OpenRecipeCommand.Execute(r);
    }
}
