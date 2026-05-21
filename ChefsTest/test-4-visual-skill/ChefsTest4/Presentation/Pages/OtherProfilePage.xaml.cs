using ChefsTest4.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation.Pages;

public sealed partial class OtherProfilePage : UserControl
{
    public OtherProfilePage() { InitializeComponent(); DataContextChanged += (_, _) => Bindings.Update(); }
    public OtherProfileViewModel? ViewModel => DataContext as OtherProfileViewModel;
}
