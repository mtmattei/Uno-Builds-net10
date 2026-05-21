using Microsoft.UI.Xaml.Controls;
namespace ChefsTest2.Presentation;
public sealed partial class LiveCookingFinishPage : Page
{
    public LiveCookingFinishPage() { this.InitializeComponent(); }
    public LiveCookingFinishViewModel? ViewModel => DataContext as LiveCookingFinishViewModel;
}
