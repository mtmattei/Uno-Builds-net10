using Microsoft.UI.Xaml.Input;

namespace ChefsTest1.Presentation;

public sealed partial class NearMePage : Page
{
    public NearMePage()
    {
        this.InitializeComponent();
    }

    public NearMeViewModel? ViewModel => DataContext as NearMeViewModel;

    private void OnChefTapped(object sender, TappedRoutedEventArgs e)
    {
        if (ViewModel?.SelectedChef is User chef)
        {
            ViewModel.OpenChefCommand.Execute(chef);
        }
    }
}
