using ChefsTest2.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest2.Presentation;

public sealed partial class CookbookDetailPage : Page
{
    public CookbookDetailPage() { this.InitializeComponent(); }
    public CookbookDetailViewModel? ViewModel => DataContext as CookbookDetailViewModel;

    private void OnRecipeClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Recipe r } && ViewModel is { } vm)
            vm.OpenRecipeCommand.Execute(r);
    }
}
