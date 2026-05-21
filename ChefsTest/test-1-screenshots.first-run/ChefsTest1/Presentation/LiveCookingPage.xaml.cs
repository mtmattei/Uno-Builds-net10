using ChefsTest1.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ChefsTest1.Presentation;

public sealed partial class LiveCookingPage : Page
{
    public LiveCookingViewModel ViewModel => (LiveCookingViewModel)DataContext;

    public LiveCookingPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RecipeData r)
            ViewModel?.Bind(r);
    }
}
