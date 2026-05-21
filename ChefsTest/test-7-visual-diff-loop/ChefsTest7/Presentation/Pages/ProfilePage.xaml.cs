namespace ChefsTest7.Presentation.Pages;

public sealed partial class ProfilePage : Microsoft.UI.Xaml.Controls.Page
{
    public ProfileViewModel? ViewModel => DataContext as ProfileViewModel;
    public ProfilePage() => this.InitializeComponent();
}
