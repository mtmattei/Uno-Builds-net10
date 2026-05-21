using ChefsTest2.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest2.Presentation;

public sealed partial class FavoritesPage : Page
{
    public FavoritesPage() { this.InitializeComponent(); }
    public FavoritesViewModel? ViewModel => DataContext as FavoritesViewModel;

    private void OnRecipeClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Recipe r } && ViewModel is { } vm)
            vm.OpenRecipeCommand.Execute(r);
    }
    private void OnCookbookClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Cookbook cb } && ViewModel is { } vm)
            vm.OpenCookbookCommand.Execute(cb);
    }
}
