namespace ChefsTest1.Presentation;

public sealed partial class NotificationsPage : Page
{
    public NotificationsPage()
    {
        this.InitializeComponent();
    }

    public NotificationsViewModel? ViewModel => DataContext as NotificationsViewModel;
}
