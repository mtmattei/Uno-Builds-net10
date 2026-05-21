using System.Linq;
using ChefsTest1.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest1.Presentation;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel => (HomeViewModel)DataContext;

    public HomePage()
    {
        this.InitializeComponent();
    }

    private void OnRecipeCardClick(object sender, RoutedEventArgs e)
    {
        // Try Tag, then DataContext, then first trending recipe as fallback
        RecipeData? r = null;
        if (sender is FrameworkElement fe)
        {
            r = fe.Tag as RecipeData ?? fe.DataContext as RecipeData;
        }
        r ??= ViewModel?.Trending.FirstOrDefault();
        if (r != null)
            ViewModel?.OpenRecipeCommand.Execute(r);
    }
}
