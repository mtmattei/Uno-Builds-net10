using System.Collections.ObjectModel;
using ChefsTest3.Models;
using ChefsTest3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest3.Presentation;

public partial class NearMeMapViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsService _service;

    public ObservableCollection<UserData> Chefs { get; } = new();

    [ObservableProperty]
    private UserData? _selected;

    public NearMeMapViewModel(INavigator navigator, IChefsService service)
    {
        _navigator = navigator;
        _service = service;
        _ = LoadAsync();
    }

    private async System.Threading.Tasks.Task LoadAsync()
    {
        var creators = await _service.GetPopularCreatorsAsync();
        Chefs.Clear();
        foreach (var u in creators) Chefs.Add(u);
        Selected = Chefs.Count > 0 ? Chefs[0] : null;
    }

    [RelayCommand]
    private void Select(UserData user)
    {
        Selected = user;
    }

    [RelayCommand]
    private async void OpenSelected()
    {
        if (Selected is null) return;
        await _navigator.NavigateViewModelAsync<OtherProfileViewModel>(this, data: Selected);
    }

    [RelayCommand]
    private async void GoBack()
    {
        await _navigator.NavigateBackAsync(this);
    }
}
