namespace ChefsTest5.Presentation;

public sealed partial class NotificationsPage : Page
{
    public NotificationsViewModel ViewModel { get; private set; } = default!;

    public NotificationsPage()
    {
        this.InitializeComponent();
        DataContextChanged += (_, __) => { if (DataContext is NotificationsViewModel vm) ViewModel = vm; };
    }
}
