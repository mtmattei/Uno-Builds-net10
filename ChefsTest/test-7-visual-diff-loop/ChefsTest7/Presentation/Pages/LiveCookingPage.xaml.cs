namespace ChefsTest7.Presentation.Pages;

public sealed partial class LiveCookingPage : Microsoft.UI.Xaml.Controls.Page
{
    public LiveCookingViewModel? ViewModel => DataContext as LiveCookingViewModel;
    public LiveCookingPage() => this.InitializeComponent();
}
