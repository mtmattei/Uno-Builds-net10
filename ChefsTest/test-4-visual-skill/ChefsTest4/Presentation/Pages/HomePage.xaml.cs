using ChefsTest4.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation.Pages;

public sealed partial class HomePage : UserControl
{
    public HomePage() { InitializeComponent(); DataContextChanged += (_, _) => Bindings.Update(); }
    public HomeViewModel? ViewModel => DataContext as HomeViewModel;
}
