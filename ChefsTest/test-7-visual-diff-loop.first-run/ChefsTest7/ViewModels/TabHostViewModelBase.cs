using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest7.ViewModels;

public partial class TabHostViewModelBase : ObservableObject
{
    protected readonly INavigator Navigator;
    public TabHostViewModelBase(INavigator navigator) { Navigator = navigator; }

    [RelayCommand] private async Task NavigateHome() => await Navigator.NavigateRouteAsync(this, "Home");
    [RelayCommand] private async Task NavigateSearch() => await Navigator.NavigateRouteAsync(this, "Search");
    [RelayCommand] private async Task NavigateFavorites() => await Navigator.NavigateRouteAsync(this, "FavoritesAllRecipes");
}
