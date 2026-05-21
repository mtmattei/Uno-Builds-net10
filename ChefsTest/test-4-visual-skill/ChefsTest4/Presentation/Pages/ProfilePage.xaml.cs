using ChefsTest4.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation.Pages;

public sealed partial class ProfilePage : UserControl
{
    public ProfilePage() { InitializeComponent(); DataContextChanged += (_, _) => Bindings.Update(); }
    public ProfileViewModel? ViewModel => DataContext as ProfileViewModel;
}
