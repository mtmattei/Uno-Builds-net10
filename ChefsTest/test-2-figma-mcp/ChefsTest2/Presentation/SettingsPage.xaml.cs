using Microsoft.UI.Xaml.Controls;
namespace ChefsTest2.Presentation;
public sealed partial class SettingsPage : Page
{
    public SettingsPage() { this.InitializeComponent(); }
    public SettingsViewModel? ViewModel => DataContext as SettingsViewModel;
}
