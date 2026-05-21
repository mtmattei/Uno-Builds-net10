using ChefsTest1.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ChefsTest1.Presentation;

public sealed partial class OtherProfilePage : Page
{
    public OtherProfileViewModel ViewModel => (OtherProfileViewModel)DataContext;

    public OtherProfilePage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is UserData u)
            ViewModel?.Bind(u);
    }

    private void OnRecipeCardClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is RecipeData r)
            ViewModel?.OpenRecipeCommand.Execute(r);
    }
}
