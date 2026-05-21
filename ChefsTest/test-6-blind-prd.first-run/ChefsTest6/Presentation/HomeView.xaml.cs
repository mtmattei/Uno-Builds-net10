using ChefsTest6.Models;

namespace ChefsTest6.Presentation;

public sealed partial class HomeView : UserControl
{
    public HomeView()
    {
        this.InitializeComponent();
        this.DataContextChanged += (s, e) =>
        {
            if (DataContext is HomeTabViewModel vm) vm.Populate();
        };
    }

    private void OnRecipeClicked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is Recipe r &&
            DataContext is HomeTabViewModel vm)
        {
            vm.OpenRecipeCommand.Execute(r);
        }
    }

    private void OnCategoryClicked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is Category c &&
            DataContext is HomeTabViewModel vm)
        {
            vm.OpenCategoryCommand.Execute(c);
        }
    }

    private void OnContributorClicked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is User u &&
            DataContext is HomeTabViewModel vm)
        {
            vm.OpenContributorCommand.Execute(u);
        }
    }
}
