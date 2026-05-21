using ChefsTest1.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ChefsTest1.Presentation;

public sealed partial class CookbookDetailPage : Page
{
    public CookbookDetailViewModel ViewModel => (CookbookDetailViewModel)DataContext;

    public CookbookDetailPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is CookbookData c)
            ViewModel?.Bind(c);
    }

    private void OnRecipeCardClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is RecipeData r)
            ViewModel?.OpenRecipeCommand.Execute(r);
    }
}
