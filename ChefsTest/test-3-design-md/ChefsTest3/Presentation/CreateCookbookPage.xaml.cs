using Microsoft.UI.Xaml.Controls;

namespace ChefsTest3.Presentation;

public sealed partial class CreateCookbookPage : Page
{
    public CreateCookbookViewModel ViewModel => (CreateCookbookViewModel)DataContext;

    public CreateCookbookPage()
    {
        this.InitializeComponent();
    }
}
