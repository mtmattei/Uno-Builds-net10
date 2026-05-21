using Microsoft.UI.Xaml.Controls;

namespace ChefsTest1.Presentation;

public sealed partial class NearMeMapPage : Page
{
    public NearMeMapViewModel ViewModel => (NearMeMapViewModel)DataContext;

    public NearMeMapPage()
    {
        this.InitializeComponent();
    }
}
