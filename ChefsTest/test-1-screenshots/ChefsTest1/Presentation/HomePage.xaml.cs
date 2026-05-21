using Microsoft.UI.Xaml.Input;

namespace ChefsTest1.Presentation;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        this.InitializeComponent();
    }

    public HomeViewModel? ViewModel => DataContext as HomeViewModel;

    private void OnRecipeTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is Recipe recipe)
        {
            ViewModel?.OpenRecipeCommand.Execute(recipe);
        }
    }

    private void OnContributorTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is User user)
        {
            ViewModel?.OpenProfileCommand.Execute(user);
        }
    }
}
