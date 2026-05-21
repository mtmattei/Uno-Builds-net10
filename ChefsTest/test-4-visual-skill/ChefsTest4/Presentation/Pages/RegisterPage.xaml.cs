using ChefsTest4.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation.Pages;

public sealed partial class RegisterPage : UserControl
{
    public RegisterPage() { InitializeComponent(); DataContextChanged += (_, _) => Bindings.Update(); }
    public RegisterViewModel? ViewModel => DataContext as RegisterViewModel;
}
