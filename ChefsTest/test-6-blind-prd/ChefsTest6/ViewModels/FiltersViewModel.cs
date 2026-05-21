namespace ChefsTest6.ViewModels;

public partial class FiltersViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly FiltersContext _context;

    [ObservableProperty]
    private CategoryData? selectedCategory;

    [ObservableProperty]
    private int? maxMinutes;

    [ObservableProperty]
    private int? selectedDifficulty;

    public IReadOnlyList<CategoryData> AllCategories { get; }
    public IReadOnlyList<int> TimeOptions { get; } = new[] { 15, 30, 60 };
    public IReadOnlyList<DifficultyOption> DifficultyOptions { get; } = new[]
    {
        new DifficultyOption(1, "Beginner"),
        new DifficultyOption(2, "Intermediate"),
        new DifficultyOption(3, "Advanced"),
    };

    public FiltersViewModel(INavigator navigator, FiltersContext context)
    {
        _navigator = navigator;
        _context = context;
        AllCategories = context.AllCategories;
        SelectedCategory = context.Category;
        MaxMinutes = context.MaxMinutes;
        SelectedDifficulty = context.Difficulty;
    }

    [RelayCommand]
    private void SelectCategory(CategoryData? c)
    {
        SelectedCategory = SelectedCategory == c ? null : c;
    }

    [RelayCommand]
    private void SelectTime(object? value)
    {
        if (TryToInt(value, out var t))
            MaxMinutes = MaxMinutes == t ? null : t;
    }

    [RelayCommand]
    private void SelectDifficulty(object? value)
    {
        if (TryToInt(value, out var d))
            SelectedDifficulty = SelectedDifficulty == d ? null : d;
    }

    private static bool TryToInt(object? value, out int result)
    {
        result = 0;
        return value switch
        {
            int i => (result = i) == result,
            string s => int.TryParse(s, out result),
            _ => false,
        };
    }

    [RelayCommand]
    private void Reset()
    {
        SelectedCategory = null;
        MaxMinutes = null;
        SelectedDifficulty = null;
    }

    [RelayCommand]
    private async Task Apply()
    {
        _context.Category = SelectedCategory;
        _context.MaxMinutes = MaxMinutes;
        _context.Difficulty = SelectedDifficulty;
        await _navigator.NavigateBackWithResultAsync(this, data: _context);
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await _navigator.NavigateBackAsync(this);
    }
}

public record DifficultyOption(int Value, string Label);
