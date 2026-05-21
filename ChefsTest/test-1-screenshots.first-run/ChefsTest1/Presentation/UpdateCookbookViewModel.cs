using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChefsTest1.Models;
using ChefsTest1.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class UpdateCookbookViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private CookbookData? cookbook;
    [ObservableProperty] private string? cookbookName;
    [ObservableProperty] private ObservableCollection<RecipeData> recipes = new();

    public UpdateCookbookViewModel(INavigator navigator, IChefsDataService data, CookbookData? cookbook = null)
    {
        _navigator = navigator;
        _data = data;
        _ = BindAsync(cookbook);
    }

    public async Task BindAsync(CookbookData? c)
    {
        Cookbook = c;
        CookbookName = c?.Name;
        var all = await _data.GetRecipesAsync();
        Recipes = new ObservableCollection<RecipeData>(all);
    }

    [RelayCommand] private Task Cancel() => _navigator.NavigateBackAsync(this);
    [RelayCommand] private Task Apply() => _navigator.NavigateBackAsync(this);
}
