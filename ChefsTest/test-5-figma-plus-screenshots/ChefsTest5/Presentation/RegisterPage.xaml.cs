namespace ChefsTest5.Presentation;

public sealed partial class RegisterPage : Page
{
    public RegisterViewModel ViewModel { get; private set; } = default!;

    public RegisterPage()
    {
        this.InitializeComponent();
        DataContextChanged += (_, __) => { if (DataContext is RegisterViewModel vm) ViewModel = vm; };
    }
}
