using System.Collections.ObjectModel;
using System.Linq;

namespace ChefsTest5.Presentation;

public partial class SearchViewModel : ObservableObject
{
    private readonly IChefsDataService _data;
    private readonly INavigator _navigator;
    private List<RecipeData> _all = new();

    [ObservableProperty] private string query = string.Empty;
    public ObservableCollection<RecipeData> Results { get; } = new();
    public int ResultCount => Results.Count;
    public bool HasResults => Results.Count > 0;

    public IAsyncRelayCommand OpenFiltersCommand { get; }

    public SearchViewModel(IChefsDataService data, INavigator navigator)
    {
        _data = data;
        _navigator = navigator;
        OpenFiltersCommand = new AsyncRelayCommand(async () =>
            await _navigator.NavigateViewModelAsync<FiltersViewModel>(this));
    }

    public async Task LoadAsync()
    {
        if (_all.Count > 0) return;
        _all = (await _data.GetRecipesAsync()).ToList();
        Refresh();
    }

    partial void OnQueryChanged(string value) => Refresh();

    private void Refresh()
    {
        Results.Clear();
        var matches = string.IsNullOrWhiteSpace(Query)
            ? _all
            : _all.Where(r => (r.Name ?? string.Empty).Contains(Query, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var r in matches) Results.Add(r);
        OnPropertyChanged(nameof(ResultCount));
        OnPropertyChanged(nameof(HasResults));
    }
}
