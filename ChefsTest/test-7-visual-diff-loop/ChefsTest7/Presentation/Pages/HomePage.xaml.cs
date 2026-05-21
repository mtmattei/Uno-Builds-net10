namespace ChefsTest7.Presentation.Pages;

public sealed partial class HomePage : Microsoft.UI.Xaml.Controls.Page
{
    public HomeViewModel? ViewModel => DataContext as HomeViewModel;
    public HomePage() => this.InitializeComponent();
}
