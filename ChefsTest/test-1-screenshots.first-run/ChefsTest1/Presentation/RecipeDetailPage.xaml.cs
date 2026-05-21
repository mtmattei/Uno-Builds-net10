using ChefsTest1.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ChefsTest1.Presentation;

public sealed partial class RecipeDetailPage : Page
{
    public RecipeDetailViewModel ViewModel => (RecipeDetailViewModel)DataContext;

    public RecipeDetailPage()
    {
        this.InitializeComponent();
        this.DataContextChanged += (_, _) => ViewModel?.SelectIngredientsCommand.Execute(null);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RecipeData r)
            ViewModel?.Bind(r);
    }
}
