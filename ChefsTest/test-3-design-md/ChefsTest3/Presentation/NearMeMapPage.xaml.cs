using ChefsTest3.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest3.Presentation;

public sealed partial class NearMeMapPage : Page
{
    public NearMeMapViewModel ViewModel => (NearMeMapViewModel)DataContext;

    public NearMeMapPage()
    {
        this.InitializeComponent();
    }

    private void OnPinClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is UserData user)
        {
            ViewModel.SelectCommand.Execute(user);
        }
    }
}
