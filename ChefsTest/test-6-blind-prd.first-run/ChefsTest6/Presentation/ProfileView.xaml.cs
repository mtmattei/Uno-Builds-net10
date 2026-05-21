using ChefsTest6.Models;

namespace ChefsTest6.Presentation;

public sealed partial class ProfileView : UserControl
{
    public ProfileView()
    {
        this.InitializeComponent();
        this.DataContextChanged += (s, e) =>
        {
            if (DataContext is ProfileTabViewModel vm) vm.Populate();
        };
    }

    private void OnRecipeClicked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is Recipe r &&
            DataContext is ProfileTabViewModel vm)
        {
            vm.OpenRecipeCommand.Execute(r);
        }
    }
}
