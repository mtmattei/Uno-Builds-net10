using System.Collections.ObjectModel;

namespace ChefsTest6.Presentation;

public sealed record NotificationGroup(string Header, IReadOnlyList<Notification> Items);

public partial class NotificationsViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefService _chef;

    public ObservableCollection<NotificationGroup> Groups { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAll), nameof(IsUnread), nameof(IsRead))]
    private int filterIndex;

    public bool IsAll => FilterIndex == 0;
    public bool IsUnread => FilterIndex == 1;
    public bool IsRead => FilterIndex == 2;

    [ObservableProperty]
    private bool isEmpty;

    public NotificationsViewModel(INavigator navigator, IChefService chef)
    {
        _navigator = navigator;
        _chef = chef;
        Refresh();
    }

    partial void OnFilterIndexChanged(int value) => Refresh();

    private void Refresh()
    {
        Groups.Clear();
        IEnumerable<Notification> source = _chef.Notifications;
        source = FilterIndex switch
        {
            1 => source.Where(n => !n.IsRead),
            2 => source.Where(n => n.IsRead),
            _ => source,
        };

        var grouped = source
            .OrderByDescending(n => n.Date)
            .GroupBy(n => RelativeHeader(n.Date))
            .Select(g => new NotificationGroup(g.Key, g.ToList()))
            .ToList();

        foreach (var g in grouped) Groups.Add(g);
        IsEmpty = grouped.Count == 0;
    }

    private static string RelativeHeader(DateTime date)
    {
        var today = DateTime.UtcNow.Date;
        var d = date.Date;
        if (d == today) return "Today";
        if (d == today.AddDays(-1)) return "Yesterday";
        if (d > today.AddDays(-7)) return d.DayOfWeek.ToString();
        return d.ToString("MMM d, yyyy");
    }

    [RelayCommand] private void SelectAll() => FilterIndex = 0;
    [RelayCommand] private void SelectUnread() => FilterIndex = 1;
    [RelayCommand] private void SelectRead() => FilterIndex = 2;

    [RelayCommand]
    private void MarkRead(Notification? n)
    {
        if (n is null) return;
        n.IsRead = true;
        Refresh();
    }

    [RelayCommand]
    private async Task CloseAsync() => await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);
}
