namespace ChefsTest7.Presentation.Pages;

public sealed partial class SearchPage : Microsoft.UI.Xaml.Controls.Page
{
    public SearchViewModel? ViewModel => DataContext as SearchViewModel;
    public SearchPage() => this.InitializeComponent();
}
