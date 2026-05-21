using Microsoft.UI.Xaml.Controls;
namespace ChefsTest2.Presentation;
public sealed partial class NotificationsPage : Page
{
    public NotificationsPage() { this.InitializeComponent(); }
    public NotificationsViewModel? ViewModel => DataContext as NotificationsViewModel;
}
