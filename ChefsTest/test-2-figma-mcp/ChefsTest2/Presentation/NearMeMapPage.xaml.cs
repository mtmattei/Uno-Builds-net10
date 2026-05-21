using Microsoft.UI.Xaml.Controls;
namespace ChefsTest2.Presentation;
public sealed partial class NearMeMapPage : Page
{
    public NearMeMapPage() { this.InitializeComponent(); }
    public NearMeMapViewModel? ViewModel => DataContext as NearMeMapViewModel;
}
