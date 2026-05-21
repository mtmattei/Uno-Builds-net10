using ChefsTest7.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace ChefsTest7.Presentation;

public sealed partial class LiveCookingFinishPage : Page
{
    public LiveCookingFinishPage() => InitializeComponent();

    private void OnStarClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is LiveCookingFinishViewModel vm && sender is Button btn && btn.CommandParameter is string s && int.TryParse(s, out int n))
        {
            vm.SetRatingCommand.Execute(n);
        }
    }
}
