using System.Collections.ObjectModel;

namespace ChefsTest6.Services;

public class NotificationService : INotificationService
{
    private readonly ObservableCollection<NotificationData> _items;

    public NotificationService()
    {
        var seed = DataLoader.Load<List<NotificationData>>("Notifications.json") ?? new();
        _items = new ObservableCollection<NotificationData>(seed);
    }

    public ObservableCollection<NotificationData> All() => _items;

    public void MarkRead(NotificationData notification) => notification.IsRead = true;
}
