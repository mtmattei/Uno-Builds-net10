using Uno.Toolkit.UI;

namespace SnackbarTestApp;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    private async void OnShowBasicClick(object sender, RoutedEventArgs e)
    {
        var reason = await SnackbarHost.ShowAsync(new SnackbarItem
        {
            Message = "Photo saved to gallery",
        });
        UpdateResult(reason);
    }

    private async void OnShowActionClick(object sender, RoutedEventArgs e)
    {
        var reason = await SnackbarHost.ShowAsync(new SnackbarItem
        {
            Message = "Item deleted",
            ActionLabel = "Undo",
        });
        UpdateResult(reason);
    }

    private async void OnShowActionDismissClick(object sender, RoutedEventArgs e)
    {
        var reason = await SnackbarHost.ShowAsync(new SnackbarItem
        {
            Message = "Connection restored",
            ActionLabel = "Retry",
            ShowDismissButton = true,
        });
        UpdateResult(reason);
    }

    private async void OnShowMultiLineClick(object sender, RoutedEventArgs e)
    {
        var reason = await SnackbarHost.ShowAsync(new SnackbarItem
        {
            Message = "This item already has the label \"travel\". You can add a new label or use the search to find existing ones.",
            ActionLabel = "Got It",
            ShowDismissButton = true,
            IsActionOnNewLine = true,
        });
        UpdateResult(reason);
    }

    private async void OnShowLongClick(object sender, RoutedEventArgs e)
    {
        var reason = await SnackbarHost.ShowAsync(new SnackbarItem
        {
            Message = "This message will stay for 7 seconds",
            Duration = SnackbarDuration.Long,
        });
        UpdateResult(reason);
    }

    private async void OnShowIndefiniteClick(object sender, RoutedEventArgs e)
    {
        var reason = await SnackbarHost.ShowAsync(new SnackbarItem
        {
            Message = "This won't go away on its own",
            ShowDismissButton = true,
            Duration = SnackbarDuration.Indefinite,
        });
        UpdateResult(reason);
    }

    private async void OnShowQueueClick(object sender, RoutedEventArgs e)
    {
        var t1 = SnackbarHost.ShowAsync(new SnackbarItem { Message = "Message 1 of 3" });
        var t2 = SnackbarHost.ShowAsync(new SnackbarItem { Message = "Message 2 of 3" });
        var t3 = SnackbarHost.ShowAsync(new SnackbarItem { Message = "Message 3 of 3" });

        var reason = await t3;
        UpdateResult(reason);
    }

    private void UpdateResult(SnackbarDismissReason reason)
    {
        ResultText.Text = $"Last dismiss reason: {reason}";
    }
}
