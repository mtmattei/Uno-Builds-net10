namespace ChefsTest7.Presentation.Pages;

public sealed partial class LiveCookingFinishPage : Microsoft.UI.Xaml.Controls.Page
{
    public LiveCookingFinishViewModel? ViewModel => DataContext as LiveCookingFinishViewModel;
    public LiveCookingFinishPage() => this.InitializeComponent();
}
