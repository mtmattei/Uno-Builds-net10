namespace ChefsTest7.Presentation.Pages;

public sealed partial class LoginPage : Microsoft.UI.Xaml.Controls.Page
{
    public LoginViewModel? ViewModel => DataContext as LoginViewModel;
    public LoginPage() => this.InitializeComponent();
}
