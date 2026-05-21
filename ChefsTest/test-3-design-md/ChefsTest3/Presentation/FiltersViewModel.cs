using System.Collections.ObjectModel;
using System.Linq;
using ChefsTest3.Models;
using ChefsTest3.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest3.Presentation;

public partial class FiltersViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IChefsService _service;

    public ObservableCollection<FilterChip> Categories { get; } = new();

    public ObservableCollection<FilterChip> CookTime { get; } = new()
    {
        new FilterChip("< 15 min", false),
        new FilterChip("15-30 min", true),
        new FilterChip("30-60 min", false),
        new FilterChip("> 60 min", false),
    };

    public ObservableCollection<FilterChip> SkillLevels { get; } = new()
    {
        new FilterChip("Easy", false),
        new FilterChip("Medium", true),
        new FilterChip("Hard", false),
    };

    public FiltersViewModel(INavigator navigator, IChefsService service)
    {
        _navigator = navigator;
        _service = service;
        _ = LoadAsync();
    }

    private async System.Threading.Tasks.Task LoadAsync()
    {
        var cats = await _service.GetCategoriesAsync();
        Categories.Clear();
        var i = 0;
        foreach (var c in cats)
        {
            Categories.Add(new FilterChip(c.Name ?? string.Empty, i++ == 0));
        }
    }

    [RelayCommand]
    private async void Apply()
    {
        await _navigator.NavigateBackAsync(this);
    }

    [RelayCommand]
    private void Reset()
    {
        foreach (var c in Categories) c.IsSelected = false;
        foreach (var c in CookTime) c.IsSelected = false;
        foreach (var c in SkillLevels) c.IsSelected = false;
    }

    [RelayCommand]
    private async void Close()
    {
        await _navigator.NavigateBackAsync(this);
    }
}

public partial class FilterChip : ObservableObject
{
    [ObservableProperty]
    private string _label;

    [ObservableProperty]
    private bool _isSelected;

    public FilterChip(string label, bool isSelected)
    {
        _label = label;
        _isSelected = isSelected;
    }

    [RelayCommand]
    private void Toggle() => IsSelected = !IsSelected;
}
