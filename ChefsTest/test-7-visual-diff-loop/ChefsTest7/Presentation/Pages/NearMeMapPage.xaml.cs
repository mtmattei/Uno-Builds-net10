namespace ChefsTest7.Presentation.Pages;

public sealed partial class NearMeMapPage : Microsoft.UI.Xaml.Controls.Page
{
    public NearMeMapViewModel? ViewModel => DataContext as NearMeMapViewModel;
    public NearMeMapPage() => this.InitializeComponent();
}
