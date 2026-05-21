using ChefsTest1.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest1.Presentation;

public sealed partial class ProfilePage : Page
{
    public ProfileViewModel ViewModel => (ProfileViewModel)DataContext;

    public ProfilePage()
    {
        this.InitializeComponent();
    }

    private void OnRecipeCardClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is RecipeData r)
            ViewModel?.OpenRecipeCommand.Execute(r);
    }
}
