using Microsoft.UI.Xaml.Controls;

namespace ChefsTest3.Presentation;

public sealed partial class RegisterPage : Page
{
    public RegisterViewModel ViewModel => (RegisterViewModel)DataContext;

    public RegisterPage()
    {
        this.InitializeComponent();
    }
}
