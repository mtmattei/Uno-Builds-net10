using Microsoft.UI.Xaml.Controls;

namespace ChefsTest3.Presentation;

public sealed partial class LiveCookingPage : Page
{
    public LiveCookingViewModel ViewModel => (LiveCookingViewModel)DataContext;

    public LiveCookingPage()
    {
        this.InitializeComponent();
    }
}
