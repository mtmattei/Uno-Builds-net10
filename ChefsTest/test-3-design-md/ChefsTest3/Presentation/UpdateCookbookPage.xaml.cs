using Microsoft.UI.Xaml.Controls;

namespace ChefsTest3.Presentation;

public sealed partial class UpdateCookbookPage : Page
{
    public UpdateCookbookViewModel ViewModel => (UpdateCookbookViewModel)DataContext;

    public UpdateCookbookPage()
    {
        this.InitializeComponent();
    }
}
