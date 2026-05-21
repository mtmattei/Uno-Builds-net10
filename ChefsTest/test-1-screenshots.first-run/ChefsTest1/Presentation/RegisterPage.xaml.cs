using Microsoft.UI.Xaml.Controls;

namespace ChefsTest1.Presentation;

public sealed partial class RegisterPage : Page
{
    public RegisterViewModel ViewModel => (RegisterViewModel)DataContext;

    public RegisterPage()
    {
        this.InitializeComponent();
    }
}
