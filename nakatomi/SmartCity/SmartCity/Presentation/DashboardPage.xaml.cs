using Microsoft.UI.Xaml.Input;

namespace SmartCity.Presentation;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        this.InitializeComponent();
    }

    // View-only twin interaction. These forward to the render control; no business logic lives here.
    private void ZoomIn_Click(object sender, RoutedEventArgs e) => Twin.ZoomIn();
    private void ZoomOut_Click(object sender, RoutedEventArgs e) => Twin.ZoomOut();

    private void Toggle2D_Click(object sender, RoutedEventArgs e)
    {
        Twin.Toggle2D();
        Toggle2DBtn.Content = Twin.Is2D ? "3D" : "2D";
    }

    private void TogglePause_Click(object sender, RoutedEventArgs e)
    {
        Twin.TogglePause();
        PauseBtn.Content = Twin.IsPaused ? "▶" : "❚❚";
    }

    // Ruler click/hover selects a floor via the shared ActiveFloorIndex DP and pins the sweep on it.
    private void RulerFloor_Click(object sender, RoutedEventArgs e) => SelectRulerFloor(sender);
    private void RulerFloor_PointerEntered(object sender, PointerRoutedEventArgs e) => SelectRulerFloor(sender);

    private void SelectRulerFloor(object sender)
    {
        if (sender is FrameworkElement { Tag: string tag } && int.TryParse(tag, out var index))
        {
            Twin.ActiveFloorIndex = index; // two-way DP → model state → tooltip + panel update
            Twin.Pause();
            PauseBtn.Content = "▶";
        }
    }
}
