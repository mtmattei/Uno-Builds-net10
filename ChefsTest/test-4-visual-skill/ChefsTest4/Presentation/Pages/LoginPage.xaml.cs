using ChefsTest4.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation.Pages;

public sealed partial class LoginPage : UserControl
{
    public LoginPage()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => Bindings.Update();
    }
    public LoginViewModel? ViewModel => DataContext as LoginViewModel;
}
