namespace ChefsTest7.Presentation.Pages;

public sealed partial class UpdateCookbookPage : Microsoft.UI.Xaml.Controls.Page
{
    public UpdateCookbookViewModel? ViewModel => DataContext as UpdateCookbookViewModel;
    public UpdateCookbookPage() => this.InitializeComponent();
}
