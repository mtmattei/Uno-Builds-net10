using ChefsTest4.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation.Pages;

public sealed partial class NotificationsPage : UserControl
{
    public NotificationsPage() { InitializeComponent(); DataContextChanged += (_, _) => Bindings.Update(); }
    public NotificationsViewModel? ViewModel => DataContext as NotificationsViewModel;
}
