using ChefsTest4.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation.Pages;

public sealed partial class NearMePage : UserControl
{
    public NearMePage() { InitializeComponent(); DataContextChanged += (_, _) => Bindings.Update(); }
    public NearMeViewModel? ViewModel => DataContext as NearMeViewModel;
}
