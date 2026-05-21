using Microsoft.UI.Xaml.Controls;

namespace ChefsTest4.Presentation;

public sealed partial class Shell : UserControl, IContentControlProvider
{
    public Shell()
    {
        InitializeComponent();
    }
    public ContentControl ContentControl => Splash;
}
