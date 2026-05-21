using ChefsTest4.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation.Pages;

public sealed partial class SettingsPage : UserControl
{
    public SettingsPage() { InitializeComponent(); DataContextChanged += (_, _) => Bindings.Update(); }
    public SettingsViewModel? ViewModel => DataContext as SettingsViewModel;
}
