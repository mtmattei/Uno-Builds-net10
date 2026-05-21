using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ChefsTest1.Models;
using ChefsTest1.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class OtherProfileViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsDataService _data;

    [ObservableProperty] private UserData? user;
    [ObservableProperty] private ObservableCollection<RecipeData> recipes = new();

    public OtherProfileViewModel(INavigator navigator, IChefsDataService data, UserData? user = null)
    {
        _navigator = navigator;
        _data = data;
        if (user != null) User = user;
        _ = Load();
    }

    private async Task Load()
    {
        if (User == null)
        {
            var users = await _data.GetUsersAsync();
            User = users.Skip(1).FirstOrDefault() ?? users.First();
        }
        var all = await _data.GetRecipesAsync();
        var theirs = all.Where(r => r.UserId == User.Id).ToList();
        Recipes = new ObservableCollection<RecipeData>(theirs);
    }

    public void Bind(UserData? u)
    {
        if (u != null) User = u;
    }

    [RelayCommand] private Task Back() => _navigator.NavigateBackAsync(this);
    [RelayCommand] private Task OpenRecipe(RecipeData? r) =>
        r == null ? Task.CompletedTask : _navigator.NavigateViewModelAsync<RecipeDetailViewModel>(this, data: r);
}
