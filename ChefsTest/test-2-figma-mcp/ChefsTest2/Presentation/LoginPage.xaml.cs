using Microsoft.UI.Xaml.Controls;

namespace ChefsTest2.Presentation;

public sealed partial class LoginPage : Page
{
    public LoginPage()
    {
        this.InitializeComponent();
    }
    public LoginViewModel? ViewModel => DataContext as LoginViewModel;
}
