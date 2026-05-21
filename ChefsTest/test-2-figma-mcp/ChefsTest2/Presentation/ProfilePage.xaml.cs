using ChefsTest2.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest2.Presentation;

public sealed partial class ProfilePage : Page
{
    public ProfilePage() { this.InitializeComponent(); }
    public ProfileViewModel? ViewModel => DataContext as ProfileViewModel;

    private void OnRecipeClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Recipe r } && ViewModel is { } vm)
            vm.OpenRecipeCommand.Execute(r);
    }
}
