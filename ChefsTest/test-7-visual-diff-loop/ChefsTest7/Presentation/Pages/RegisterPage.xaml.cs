namespace ChefsTest7.Presentation.Pages;

public sealed partial class RegisterPage : Microsoft.UI.Xaml.Controls.Page
{
    public RegisterViewModel? ViewModel => DataContext as RegisterViewModel;
    public RegisterPage() => this.InitializeComponent();
}
