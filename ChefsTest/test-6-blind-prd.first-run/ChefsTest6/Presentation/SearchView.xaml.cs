using ChefsTest6.Models;

namespace ChefsTest6.Presentation;

public sealed partial class SearchView : UserControl
{
    public SearchView()
    {
        this.InitializeComponent();
        this.DataContextChanged += (s, e) =>
        {
            if (DataContext is SearchTabViewModel vm) vm.Populate();
        };
    }

    private void OnRecipeClicked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is Recipe r &&
            DataContext is SearchTabViewModel vm)
        {
            vm.OpenRecipeCommand.Execute(r);
        }
    }
}
