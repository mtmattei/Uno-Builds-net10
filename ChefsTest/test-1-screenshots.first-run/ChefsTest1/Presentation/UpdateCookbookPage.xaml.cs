using ChefsTest1.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ChefsTest1.Presentation;

public sealed partial class UpdateCookbookPage : Page
{
    public UpdateCookbookViewModel ViewModel => (UpdateCookbookViewModel)DataContext;

    public UpdateCookbookPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is CookbookData c && ViewModel != null)
            await ViewModel.BindAsync(c);
    }
}
