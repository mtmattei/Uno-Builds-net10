using ChefsTest2.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest2.Presentation;

public sealed partial class OtherProfilePage : Page
{
    public OtherProfilePage() { this.InitializeComponent(); }
    public OtherProfileViewModel? ViewModel => DataContext as OtherProfileViewModel;

    private void OnRecipeClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Recipe r } && ViewModel is { } vm)
            vm.OpenRecipeCommand.Execute(r);
    }
}
