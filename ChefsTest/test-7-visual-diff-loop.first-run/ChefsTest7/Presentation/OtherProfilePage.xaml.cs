using ChefsTest7.Models;
using ChefsTest7.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest7.Presentation;

public sealed partial class OtherProfilePage : Page
{
    public OtherProfilePage() => InitializeComponent();
    private void OnRecipeClick(object sender, ItemClickEventArgs e)
    {
        if (DataContext is OtherProfileViewModel vm && e.ClickedItem is RecipeData r)
            vm.OpenRecipeCommand.Execute(r);
    }
}
