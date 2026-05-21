namespace ChefsTest7.Presentation.Pages;

public sealed partial class OtherProfilePage : Microsoft.UI.Xaml.Controls.Page
{
    public OtherProfileViewModel? ViewModel => DataContext as OtherProfileViewModel;
    public OtherProfilePage() => this.InitializeComponent();
}
