using System.Collections.ObjectModel;

namespace ChefsTest6.Presentation;

public partial class SearchTabViewModel : ObservableObject
{
    private readonly MainViewModel _parent;
    private readonly IChefService _chef;

    public ObservableCollection<Recipe> Results { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ResultText))]
    private string? query;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ResultText), nameof(IsFilterActive))]
    private SearchFilters filters = SearchFilters.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResults), nameof(IsEmpty), nameof(ResultText))]
    private int resultCount;

    public bool HasResults => ResultCount > 0;
    public bool IsEmpty => Results.Count == 0;
    public bool IsFilterActive => Filters.IsActive;
    public string ResultText => $"{ResultCount} {(ResultCount == 1 ? "result" : "results")}";

    public SearchTabViewModel(MainViewModel parent, IChefService chef)
    {
        _parent = parent;
        _chef = chef;
    }

    partial void OnQueryChanged(string? value) => Refresh();
    partial void OnFiltersChanged(SearchFilters value) => Refresh();

    public void Populate()
    {
        Refresh();
    }

    public void Refresh()
    {
        Results.Clear();
        foreach (var r in _chef.Search(Query, Filters)) Results.Add(r);
        ResultCount = Results.Count;
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasResults));
    }

    [RelayCommand]
    private async Task OpenFiltersAsync()
    {
        await _parent.Navigator.NavigateViewModelAsync<FiltersViewModel>(this, data: this);
    }

    [RelayCommand]
    private void ClearFilters()
    {
        Filters = SearchFilters.Empty;
    }

    public IAsyncRelayCommand<Recipe?> OpenRecipeCommand => _parent.OpenRecipeCommand;
    public IRelayCommand<Recipe?> ToggleFavoriteCommand => _parent.ToggleFavoriteCommand;
}
