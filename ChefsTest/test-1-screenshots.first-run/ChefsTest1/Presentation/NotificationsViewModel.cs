using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ChefsTest1.Models;
using ChefsTest1.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class NotificationsViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;
    private List<NotificationData> _all = new();

    [ObservableProperty] private ObservableCollection<NotificationData> notifications = new();
    [ObservableProperty] private bool isAllTab = true;
    [ObservableProperty] private bool isUnreadTab;
    [ObservableProperty] private bool isReadTab;
    [ObservableProperty] private bool isEmpty;

    public NotificationsViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        _ = Load();
    }

    private async Task Load()
    {
        _all = (await _data.GetNotificationsAsync()).ToList();
        Apply();
    }

    private void Apply()
    {
        var filtered = IsUnreadTab ? _all.Where(n => !n.IsRead).ToList()
                     : IsReadTab ? _all.Where(n => n.IsRead).ToList()
                     : _all;
        Notifications = new ObservableCollection<NotificationData>(filtered);
        IsEmpty = filtered.Count == 0;
    }

    [RelayCommand] private void ShowAll() { IsAllTab = true; IsUnreadTab = false; IsReadTab = false; Apply(); }
    [RelayCommand] private void ShowUnread() { IsAllTab = false; IsUnreadTab = true; IsReadTab = false; Apply(); }
    [RelayCommand] private void ShowRead() { IsAllTab = false; IsUnreadTab = false; IsReadTab = true; Apply(); }
    [RelayCommand] private Task Close() => _navigator.NavigateBackAsync(this);
}
