namespace ChefsTest7.Presentation.Pages;

public sealed partial class SettingsPage : Microsoft.UI.Xaml.Controls.Page
{
    public SettingsViewModel? ViewModel => DataContext as SettingsViewModel;
    public SettingsPage() => this.InitializeComponent();
}
