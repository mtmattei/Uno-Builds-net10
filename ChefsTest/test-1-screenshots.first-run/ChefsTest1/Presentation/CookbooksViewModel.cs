using System.Threading.Tasks;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class CookbooksViewModel
{
    private readonly INavigator _navigator;

    public CookbooksViewModel(INavigator navigator)
    {
        _navigator = navigator;
        _ = Redirect();
    }

    private async Task Redirect()
    {
        // Cookbooks tab is part of FavoritesPage; this VM just ensures the route exists
        // and forwards the user there (Favorites VM defaults its tab via this entry).
        await Task.Delay(1);
        await _navigator.NavigateViewModelAsync<FavoritesViewModel>(this, qualifier: Qualifiers.ClearBackStack);
    }
}
