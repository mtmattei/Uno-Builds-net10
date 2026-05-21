using Microsoft.UI.Xaml.Controls;

namespace ChefsTest3.Presentation;

public sealed partial class NotificationsPage : Page
{
    public NotificationsViewModel ViewModel => (NotificationsViewModel)DataContext;

    public NotificationsPage()
    {
        this.InitializeComponent();
    }
}
