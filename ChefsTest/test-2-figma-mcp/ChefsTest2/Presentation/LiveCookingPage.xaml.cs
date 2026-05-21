using Microsoft.UI.Xaml.Controls;
namespace ChefsTest2.Presentation;
public sealed partial class LiveCookingPage : Page
{
    public LiveCookingPage() { this.InitializeComponent(); }
    public LiveCookingViewModel? ViewModel => DataContext as LiveCookingViewModel;
}
