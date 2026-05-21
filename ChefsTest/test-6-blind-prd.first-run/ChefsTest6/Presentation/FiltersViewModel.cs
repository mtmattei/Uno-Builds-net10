namespace ChefsTest6.Presentation;

public partial class FiltersViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly SearchTabViewModel? _target;

    [ObservableProperty]
    private FilterCategory category;

    [ObservableProperty]
    private FilterCookingTime cookingTime;

    [ObservableProperty]
    private FilterSkill skill;

    public bool IsCategoryPopular => Category == FilterCategory.Popular;
    public bool IsCategoryTrending => Category == FilterCategory.Trending;
    public bool IsCategoryRecent => Category == FilterCategory.Recent;

    public bool IsTimeUnder15 => CookingTime == FilterCookingTime.Under15;
    public bool IsTimeUnder30 => CookingTime == FilterCookingTime.Under30;
    public bool IsTimeUnder60 => CookingTime == FilterCookingTime.Under60;

    public bool IsSkillBeginner => Skill == FilterSkill.Beginner;
    public bool IsSkillIntermediate => Skill == FilterSkill.Intermediate;
    public bool IsSkillAdvanced => Skill == FilterSkill.Advanced;

    public FiltersViewModel(INavigator navigator, SearchTabViewModel target)
    {
        _navigator = navigator;
        _target = target;
        if (target.Filters is { } current)
        {
            Category = current.Category;
            CookingTime = current.CookingTime;
            Skill = current.Skill;
        }
    }

    partial void OnCategoryChanged(FilterCategory value) => RaiseChipFlags();
    partial void OnCookingTimeChanged(FilterCookingTime value) => RaiseChipFlags();
    partial void OnSkillChanged(FilterSkill value) => RaiseChipFlags();

    private void RaiseChipFlags()
    {
        OnPropertyChanged(nameof(IsCategoryPopular));
        OnPropertyChanged(nameof(IsCategoryTrending));
        OnPropertyChanged(nameof(IsCategoryRecent));
        OnPropertyChanged(nameof(IsTimeUnder15));
        OnPropertyChanged(nameof(IsTimeUnder30));
        OnPropertyChanged(nameof(IsTimeUnder60));
        OnPropertyChanged(nameof(IsSkillBeginner));
        OnPropertyChanged(nameof(IsSkillIntermediate));
        OnPropertyChanged(nameof(IsSkillAdvanced));
    }

    [RelayCommand] private void SetCategoryPopular() => Category = Category == FilterCategory.Popular ? FilterCategory.None : FilterCategory.Popular;
    [RelayCommand] private void SetCategoryTrending() => Category = Category == FilterCategory.Trending ? FilterCategory.None : FilterCategory.Trending;
    [RelayCommand] private void SetCategoryRecent() => Category = Category == FilterCategory.Recent ? FilterCategory.None : FilterCategory.Recent;

    [RelayCommand] private void SetTimeUnder15() => CookingTime = CookingTime == FilterCookingTime.Under15 ? FilterCookingTime.None : FilterCookingTime.Under15;
    [RelayCommand] private void SetTimeUnder30() => CookingTime = CookingTime == FilterCookingTime.Under30 ? FilterCookingTime.None : FilterCookingTime.Under30;
    [RelayCommand] private void SetTimeUnder60() => CookingTime = CookingTime == FilterCookingTime.Under60 ? FilterCookingTime.None : FilterCookingTime.Under60;

    [RelayCommand] private void SetSkillBeginner() => Skill = Skill == FilterSkill.Beginner ? FilterSkill.None : FilterSkill.Beginner;
    [RelayCommand] private void SetSkillIntermediate() => Skill = Skill == FilterSkill.Intermediate ? FilterSkill.None : FilterSkill.Intermediate;
    [RelayCommand] private void SetSkillAdvanced() => Skill = Skill == FilterSkill.Advanced ? FilterSkill.None : FilterSkill.Advanced;

    [RelayCommand]
    private void Reset()
    {
        Category = FilterCategory.None;
        CookingTime = FilterCookingTime.None;
        Skill = FilterSkill.None;
    }

    [RelayCommand]
    private async Task ApplyAsync()
    {
        if (_target is not null)
        {
            _target.Filters = new SearchFilters(Category, CookingTime, Skill);
        }
        await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);
    }

    [RelayCommand]
    private async Task CloseAsync() => await _navigator.NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack);
}
