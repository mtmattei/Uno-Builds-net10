namespace ChefsTest7.Presentation.Pages;

public sealed partial class MainShellPage : Microsoft.UI.Xaml.Controls.Page
{
    public MainShellViewModel? ViewModel => DataContext as MainShellViewModel;
    public MainShellPage() => this.InitializeComponent();
}
