using ChefsTest4.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation.Pages;

public sealed partial class LiveCookingPage : UserControl
{
    public LiveCookingPage() { InitializeComponent(); DataContextChanged += (_, _) => Bindings.Update(); }
    public LiveCookingViewModel? ViewModel => DataContext as LiveCookingViewModel;
}
