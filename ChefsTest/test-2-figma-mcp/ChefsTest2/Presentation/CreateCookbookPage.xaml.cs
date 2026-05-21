using ChefsTest2.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest2.Presentation;

public sealed partial class CreateCookbookPage : Page
{
    public CreateCookbookPage() { this.InitializeComponent(); }
    public CreateCookbookViewModel? ViewModel => DataContext as CreateCookbookViewModel;

    private void OnToggleClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Recipe r } && ViewModel is { } vm)
            vm.ToggleRecipeCommand.Execute(r);
    }
}
