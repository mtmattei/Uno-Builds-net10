using Microsoft.UI.Xaml.Controls;

namespace ChefsTest1.Presentation;

public sealed partial class CreateCookbookPage : Page
{
    public CreateCookbookViewModel ViewModel => (CreateCookbookViewModel)DataContext;

    public CreateCookbookPage()
    {
        this.InitializeComponent();
    }
}
