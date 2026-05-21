namespace ChefsTest1.Presentation;

public sealed partial class RegisterPage : Page
{
    public RegisterPage()
    {
        this.InitializeComponent();
    }

    public RegisterViewModel? ViewModel => DataContext as RegisterViewModel;
}
