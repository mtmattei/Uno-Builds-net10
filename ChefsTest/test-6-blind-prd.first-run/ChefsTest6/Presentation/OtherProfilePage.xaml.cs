using ChefsTest6.Models;

namespace ChefsTest6.Presentation;

public sealed partial class OtherProfilePage : Page
{
    public OtherProfilePage()
    {
        this.InitializeComponent();
    }

    private void OnRecipeClicked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is Recipe r &&
            DataContext is OtherProfileViewModel vm)
        {
            vm.OpenRecipeCommand.Execute(r);
        }
    }
}
