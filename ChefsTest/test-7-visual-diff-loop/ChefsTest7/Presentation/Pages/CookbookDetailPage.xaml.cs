namespace ChefsTest7.Presentation.Pages;

public sealed partial class CookbookDetailPage : Microsoft.UI.Xaml.Controls.Page
{
    public CookbookDetailViewModel? ViewModel => DataContext as CookbookDetailViewModel;
    public CookbookDetailPage() => this.InitializeComponent();
}
