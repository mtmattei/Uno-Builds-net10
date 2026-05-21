using ChefsTest4.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation.Pages;

public sealed partial class LiveCookingFinishPage : UserControl
{
    public LiveCookingFinishPage() { InitializeComponent(); DataContextChanged += (_, _) => Bindings.Update(); }
    public LiveCookingFinishViewModel? ViewModel => DataContext as LiveCookingFinishViewModel;
}
