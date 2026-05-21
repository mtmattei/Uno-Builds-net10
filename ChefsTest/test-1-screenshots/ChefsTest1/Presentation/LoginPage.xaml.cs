namespace ChefsTest1.Presentation;

public sealed partial class LoginPage : Page
{
    public LoginPage()
    {
        this.InitializeComponent();
    }

    public LoginViewModel? ViewModel => DataContext as LoginViewModel;
}
