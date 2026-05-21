using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace ChefsTest1.Presentation;

public partial class FiltersViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty] private string? selectedCategory = "Popular";
    [ObservableProperty] private string? selectedTime = "15 min";
    [ObservableProperty] private string? selectedSkill = "Beginner";

    public FiltersViewModel(INavigator navigator)
    {
        _navigator = navigator;
    }

    [RelayCommand]
    private void SetCategory(string? value) => SelectedCategory = value;
    [RelayCommand]
    private void SetTime(string? value) => SelectedTime = value;
    [RelayCommand]
    private void SetSkill(string? value) => SelectedSkill = value;

    [RelayCommand]
    private void Reset()
    {
        SelectedCategory = null;
        SelectedTime = null;
        SelectedSkill = null;
    }

    [RelayCommand]
    private Task Apply() => _navigator.NavigateBackAsync(this);

    [RelayCommand]
    private Task Close() => _navigator.NavigateBackAsync(this);
}
