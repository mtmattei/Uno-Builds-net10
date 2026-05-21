using System.Collections.ObjectModel;
using System.Linq;
using ChefsTest3.Models;
using ChefsTest3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest3.Presentation;

public partial class NotificationsViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsService _service;
    private System.Collections.Generic.IReadOnlyList<NotificationData> _all = System.Array.Empty<NotificationData>();

    public ObservableCollection<NotificationData> Items { get; } = new();

    [ObservableProperty]
    private string _activeFilter = "All";

    [ObservableProperty]
    private bool _hasItems = true;

    public NotificationsViewModel(INavigator navigator, IChefsService service)
    {
        _navigator = navigator;
        _service = service;
        _ = LoadAsync();
    }

    private async System.Threading.Tasks.Task LoadAsync()
    {
        _all = await _service.GetNotificationsAsync();
        Refresh();
    }

    private void Refresh()
    {
        Items.Clear();
        var filtered = ActiveFilter switch
        {
            "Unread" => _all.Where(n => !n.IsRead),
            "Read" => _all.Where(n => n.IsRead),
            _ => _all,
        };
        foreach (var n in filtered) Items.Add(n);
        HasItems = Items.Count > 0;
    }

    [RelayCommand]
    private void Filter(string filter)
    {
        ActiveFilter = filter;
        Refresh();
    }

    [RelayCommand]
    private async void Close()
    {
        await _navigator.NavigateBackAsync(this);
    }
}
