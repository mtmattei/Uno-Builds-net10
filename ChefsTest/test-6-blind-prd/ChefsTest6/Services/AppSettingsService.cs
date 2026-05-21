namespace ChefsTest6.Services;

public class AppSettingsService : IAppSettingsService
{
    private bool _notifications = true;
    private bool _nightMode;

    public bool NotificationsEnabled
    {
        get => _notifications;
        set => _notifications = value;
    }

    public bool NightMode
    {
        get => _nightMode;
        set
        {
            if (_nightMode == value) return;
            _nightMode = value;
            NightModeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? NightModeChanged;
}
