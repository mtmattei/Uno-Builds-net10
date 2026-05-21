using OrbFidget.Controls;
using OrbFidget.Services;
using Uno.WinUI.Graphics2DSK;

namespace OrbFidget;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!SKCanvasElement.IsSupportedOnCurrentPlatform())
        {
            RootGrid.Children.Add(new TextBlock
            {
                Text = "SKCanvasElement is not supported on this platform.",
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
            return;
        }

        var haptics = new OrbHapticService();
        await haptics.InitializeAsync();

        var audio = new OrbAudioEngine();

        var orbCanvas = new OrbCanvas(haptics, audio)
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        RootGrid.Children.Add(orbCanvas);
    }
}
