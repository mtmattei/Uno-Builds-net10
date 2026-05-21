using System.Collections.ObjectModel;

namespace ChefsTest6.ViewModels;

public partial class NotificationsViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly INotificationService _service;

    public ObservableCollection<NotificationGroup> Groups { get; } = new();

    [ObservableProperty]
    private int selectedSegment;

    public NotificationsViewModel(INavigator navigator, INotificationService service)
    {
        _navigator = navigator;
        _service = service;
        Rebuild();
    }

    partial void OnSelectedSegmentChanged(int value) => Rebuild();

    private void Rebuild()
    {
        Groups.Clear();
        var all = _service.All().ToList();
        var filtered = SelectedSegment switch
        {
            1 => all.Where(n => !n.IsRead),
            2 => all.Where(n => n.IsRead),
            _ => all
        };
        var ordered = filtered.OrderByDescending(n => n.Date);
        foreach (var bucket in ordered.GroupBy(n => n.RelativeBucket))
        {
            var g = new NotificationGroup { Title = bucket.Key };
            foreach (var item in bucket) g.Items.Add(item);
            Groups.Add(g);
        }
        OnPropertyChanged(nameof(HasItems));
    }

    public bool HasItems => Groups.Any(g => g.Items.Count > 0);

    [RelayCommand]
    private void MarkRead(NotificationData? n)
    {
        if (n is null) return;
        _service.MarkRead(n);
    }

    [RelayCommand]
    private async Task Close() => await _navigator.NavigateBackAsync(this);

    [RelayCommand]
    private void SelectSegment(string? value)
    {
        if (int.TryParse(value, out var i)) SelectedSegment = i;
    }
}

public class NotificationGroup
{
    public string Title { get; set; } = string.Empty;
    public ObservableCollection<NotificationData> Items { get; } = new();
}
