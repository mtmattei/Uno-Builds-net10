using ChefsTest6.Models;

namespace ChefsTest6.Presentation;

public sealed partial class FavoritesView : UserControl
{
    public FavoritesView()
    {
        this.InitializeComponent();
        this.DataContextChanged += (s, e) =>
        {
            if (DataContext is FavoritesTabViewModel vm) vm.Populate();
        };
    }

    private void OnRecipeClicked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is Recipe r &&
            DataContext is FavoritesTabViewModel vm)
        {
            vm.OpenRecipeCommand.Execute(r);
        }
    }

    private void OnCookbookClicked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is Cookbook c &&
            DataContext is FavoritesTabViewModel vm)
        {
            vm.OpenCookbookCommand.Execute(c);
        }
    }
}
