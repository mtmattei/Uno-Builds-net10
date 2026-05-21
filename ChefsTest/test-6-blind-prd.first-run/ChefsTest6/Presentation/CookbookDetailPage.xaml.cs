using ChefsTest6.Models;

namespace ChefsTest6.Presentation;

public sealed partial class CookbookDetailPage : Page
{
    public CookbookDetailPage()
    {
        this.InitializeComponent();
    }

    private void OnRecipeClicked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is Recipe r &&
            DataContext is CookbookDetailViewModel vm)
        {
            vm.OpenRecipeCommand.Execute(r);
        }
    }
}
