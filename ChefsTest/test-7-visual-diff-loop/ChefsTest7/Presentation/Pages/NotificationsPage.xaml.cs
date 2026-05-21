namespace ChefsTest7.Presentation.Pages;

public sealed partial class NotificationsPage : Microsoft.UI.Xaml.Controls.Page
{
    public NotificationsViewModel? ViewModel => DataContext as NotificationsViewModel;
    public NotificationsPage() => this.InitializeComponent();
}
