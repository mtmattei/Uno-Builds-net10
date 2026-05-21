using Microsoft.UI.Xaml.Input;

namespace ChefsTest1.Presentation;

public sealed partial class FavoritesPage : Page
{
    public FavoritesPage()
    {
        this.InitializeComponent();
    }

    public FavoritesViewModel? ViewModel => DataContext as FavoritesViewModel;

    private void OnRecipeTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is Recipe recipe)
        {
            ViewModel?.OpenRecipeCommand.Execute(recipe);
        }
    }

    private void OnCookbookTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is Cookbook cookbook)
        {
            ViewModel?.OpenCookbookCommand.Execute(cookbook);
        }
    }
}
