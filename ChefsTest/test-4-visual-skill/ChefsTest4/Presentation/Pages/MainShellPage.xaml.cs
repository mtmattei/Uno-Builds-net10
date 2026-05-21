using ChefsTest4.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation.Pages;

public sealed partial class MainShellPage : UserControl
{
    public MainShellPage() { InitializeComponent(); DataContextChanged += (_, _) => Bindings.Update(); }
    public MainShellViewModel? ViewModel => DataContext as MainShellViewModel;
}
