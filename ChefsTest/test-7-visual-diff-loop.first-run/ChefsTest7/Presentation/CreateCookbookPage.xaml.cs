using ChefsTest7.Models;
using ChefsTest7.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest7.Presentation;

public sealed partial class CreateCookbookPage : Page
{
    public CreateCookbookPage() => InitializeComponent();
    private void OnPick(object sender, ItemClickEventArgs e)
    {
        if (DataContext is CreateCookbookViewModel vm && e.ClickedItem is RecipeData r)
            vm.ToggleSelectCommand.Execute(r);
    }
}
