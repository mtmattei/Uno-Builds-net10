using Microsoft.UI.Xaml.Controls;

namespace ChefsTest2.Presentation;

public sealed partial class RegisterPage : Page
{
    public RegisterPage() { this.InitializeComponent(); }
    public RegisterViewModel? ViewModel => DataContext as RegisterViewModel;
}
