using System.Collections.ObjectModel;

namespace ChefsTest1.Presentation;

public partial class NotificationsViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly INotificationService _notifications;

    public ObservableCollection<Notification> Items { get; } = new();
    public ObservableCollection<Notification> All { get; } = new();
    public ObservableCollection<Notification> Unread { get; } = new();
    public ObservableCollection<Notification> Read { get; } = new();

    [ObservableProperty] private int selectedTabIndex; // 0=all 1=unread 2=read

    public bool IsAll => SelectedTabIndex == 0;
    public bool IsUnread => SelectedTabIndex == 1;
    public bool IsRead => SelectedTabIndex == 2;
    public bool HasItems => Items.Count > 0;
    public bool HasNoItems => Items.Count == 0;

    public NotificationsViewModel(INavigator navigator, INotificationService notifications)
    {
        _navigator = navigator;
        _notifications = notifications;
        CloseCommand = new AsyncRelayCommand(() => _navigator.NavigateBackAsync(this));
        SelectAllCommand = new RelayCommand(() => { SelectedTabIndex = 0; Apply(); });
        SelectUnreadCommand = new RelayCommand(() => { SelectedTabIndex = 1; Apply(); });
        SelectReadCommand = new RelayCommand(() => { SelectedTabIndex = 2; Apply(); });
        _ = LoadAsync();
    }

    public ICommand CloseCommand { get; }
    public ICommand SelectAllCommand { get; }
    public ICommand SelectUnreadCommand { get; }
    public ICommand SelectReadCommand { get; }

    private async Task LoadAsync()
    {
        var list = await _notifications.GetAllAsync();
        foreach (var n in list) All.Add(n);
        foreach (var n in list.Where(x => !x.IsRead)) Unread.Add(n);
        foreach (var n in list.Where(x => x.IsRead)) Read.Add(n);
        Apply();
    }

    private void Apply()
    {
        Items.Clear();
        var src = SelectedTabIndex switch
        {
            1 => Unread,
            2 => Read,
            _ => All
        };
        foreach (var n in src) Items.Add(n);
        OnPropertyChanged(nameof(HasItems));
        OnPropertyChanged(nameof(HasNoItems));
        OnPropertyChanged(nameof(IsAll));
        OnPropertyChanged(nameof(IsUnread));
        OnPropertyChanged(nameof(IsRead));
    }
}
