using Microsoft.UI.Xaml.Input;

namespace ChefsTest1.Presentation;

public sealed partial class SearchPage : Page
{
    public SearchPage()
    {
        this.InitializeComponent();
    }

    public SearchViewModel? ViewModel => DataContext as SearchViewModel;

    private void OnRecipeTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is Recipe recipe)
        {
            ViewModel?.OpenRecipeCommand.Execute(recipe);
        }
    }
}
