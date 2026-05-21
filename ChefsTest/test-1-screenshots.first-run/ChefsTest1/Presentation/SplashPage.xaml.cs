using Microsoft.UI.Xaml.Controls;

namespace ChefsTest1.Presentation;

public sealed partial class SplashPage : Page
{
    public SplashViewModel ViewModel => (SplashViewModel)DataContext;

    public SplashPage()
    {
        this.InitializeComponent();
    }
}
