using ChefsTest2.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest2.Presentation;

public sealed partial class HomePage : Page
{
    public HomePage() { this.InitializeComponent(); }
    public HomeViewModel? ViewModel => DataContext as HomeViewModel;

    private void OnTrendingClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Recipe r } && ViewModel is { } vm)
            vm.OpenRecipeCommand.Execute(r);
    }
    private void OnCategoryClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Category c } && ViewModel is { } vm)
            vm.OpenCategoryCommand.Execute(c);
    }
    private void OnContributorClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: User u } && ViewModel is { } vm)
            vm.OpenContributorCommand.Execute(u);
    }
}
