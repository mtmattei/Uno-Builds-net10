namespace ChefsTest1.Presentation;

public partial class FiltersViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty] private string? selectedCategory;
    [ObservableProperty] private string? selectedCookTime;
    [ObservableProperty] private string? selectedSkill;

    public FiltersViewModel(INavigator navigator)
    {
        _navigator = navigator;
        CloseCommand = new AsyncRelayCommand(() => _navigator.NavigateBackAsync(this));
        ResetCommand = new RelayCommand(Reset);
        ApplyCommand = new AsyncRelayCommand(Apply);
        SelectCategoryCommand = new RelayCommand<string>(c => SelectedCategory = c);
        SelectCookTimeCommand = new RelayCommand<string>(c => SelectedCookTime = c);
        SelectSkillCommand = new RelayCommand<string>(c => SelectedSkill = c);
    }

    public ICommand CloseCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand ApplyCommand { get; }
    public ICommand SelectCategoryCommand { get; }
    public ICommand SelectCookTimeCommand { get; }
    public ICommand SelectSkillCommand { get; }

    private void Reset()
    {
        SelectedCategory = null;
        SelectedCookTime = null;
        SelectedSkill = null;
    }

    private async Task Apply()
    {
        await _navigator.NavigateBackAsync(this);
    }
}
