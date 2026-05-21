namespace ChefsTest7.Presentation.Pages;

public sealed partial class CreateCookbookPage : Microsoft.UI.Xaml.Controls.Page
{
    public CreateCookbookViewModel? ViewModel => DataContext as CreateCookbookViewModel;
    public CreateCookbookPage() => this.InitializeComponent();
}
