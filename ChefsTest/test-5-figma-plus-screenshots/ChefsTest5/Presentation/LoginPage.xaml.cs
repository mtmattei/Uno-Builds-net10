namespace ChefsTest5.Presentation;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel { get; private set; } = default!;

    public LoginPage()
    {
        this.InitializeComponent();
        DataContextChanged += (_, __) => { if (DataContext is LoginViewModel vm) ViewModel = vm; };
    }
}
