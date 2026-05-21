using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChefsTest1.Models;
using ChefsTest1.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class NearMeMapViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private UserData? featuredCreator;

    public NearMeMapViewModel(INavigator navigator, IChefsDataService data)
    {
        _navigator = navigator;
        _data = data;
        _ = Load();
    }

    private async Task Load()
    {
        var creators = await _data.GetPopularCreatorsAsync();
        FeaturedCreator = creators.Count > 1 ? creators[1] : creators[0];
    }

    [RelayCommand] private Task Back() => _navigator.NavigateBackAsync(this);
    [RelayCommand] private Task ViewCreator() =>
        FeaturedCreator == null ? Task.CompletedTask : _navigator.NavigateViewModelAsync<OtherProfileViewModel>(this, data: FeaturedCreator);
}
