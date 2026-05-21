using ChefsTest2.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest2.Presentation;

public sealed partial class SearchPage : Page
{
    public SearchPage() { this.InitializeComponent(); }
    public SearchViewModel? ViewModel => DataContext as SearchViewModel;

    private void OnRecipeClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Recipe r } && ViewModel is { } vm)
            vm.OpenRecipeCommand.Execute(r);
    }
}
