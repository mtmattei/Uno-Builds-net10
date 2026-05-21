using System.Collections.ObjectModel;
using System.Linq;

namespace ChefsTest5.Presentation;

public partial class NotificationsViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;
    private List<NotificationData> _all = new();

    [ObservableProperty] private int filterIndex;

    public ObservableCollection<NotificationData> Notifications { get; } = new();
    public int NotificationCount => Notifications.Count;

    public IRelayCommand<string> SelectFilterCommand { get; }
    public IAsyncRelayCommand BackCommand { get; }

    public NotificationsViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        SelectFilterCommand = new RelayCommand<string>(s => { if (int.TryParse(s, out var i)) FilterIndex = i; });
        BackCommand = new AsyncRelayCommand(async () => await _navigator.NavigateBackAsync(this));
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        _all = (await _data.GetNotificationsAsync()).ToList();
        Refresh();
    }

    partial void OnFilterIndexChanged(int value) => Refresh();

    private void Refresh()
    {
        Notifications.Clear();
        var filtered = FilterIndex switch
        {
            1 => _all.Where(n => !n.IsRead),
            2 => _all.Where(n => n.IsRead),
            _ => _all,
        };
        foreach (var n in filtered) Notifications.Add(n);
        OnPropertyChanged(nameof(NotificationCount));
    }
}
