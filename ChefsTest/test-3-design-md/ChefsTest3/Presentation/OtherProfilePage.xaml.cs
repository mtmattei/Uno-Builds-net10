using ChefsTest3.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest3.Presentation;

public sealed partial class OtherProfilePage : Page
{
    public OtherProfileViewModel ViewModel => (OtherProfileViewModel)DataContext;

    public OtherProfilePage()
    {
        this.InitializeComponent();
    }

    private void OnRecipeClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is RecipeData recipe)
        {
            ViewModel.OpenRecipeCommand.Execute(recipe);
        }
    }
}
