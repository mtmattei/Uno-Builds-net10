using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using SmartCity.Models;
using SmartCity.Services;

namespace SmartCity.Presentation.Controls;

/// <summary>
/// GPU-backed building twin (M3). Draws a stacked-box tower as a wireframe via SkiaSharp on the
/// Uno Skia renderer (SKCanvasElement). The projection/camera are ours — this step is the wireframe
/// proof on the preview SkiaSharp 4.x pin; the SKMesh glass/glow upgrade lands on top of it next.
/// Floor 14 is flagged and drawn in the optimization-green accent.
/// </summary>
public sealed partial class BuildingTwinView : SKCanvasElement
{
    // Accent colours mirror the named theme brushes (TwinBodyBlue / TwinGlowGreen). SkiaSharp draws
    // in SKColor, so they live here as constants resolved at draw time (per the design brief).
    private static readonly SKColor BodyBlue = new(0x5B, 0x82, 0xFF);
    private static readonly SKColor BodyBlueDim = new(0x3A, 0x52, 0xB0);
    private static readonly SKColor GlowGreen = new(0x46, 0xE0, 0x8C);

    private readonly IReadOnlyList<FloorBox> _floors;
    private readonly DispatcherTimer _timer;

    // Camera: orbit azimuth (animated) + fixed elevation, simple perspective.
    private float _azimuth = -0.62f;       // ~ -35° three-quarter view, matches the reference
    private const float Elevation = 0.30f; // ~ 17° downward tilt
    private const float CamDistance = 6.0f;

    public BuildingTwinView()
    {
        // 16-storey tower; flag floor 14 (the AI-suggestion target) so it glows green.
        _floors = new BoxStackGeometryProvider().BuildTower(floorCount: 16, flaggedFloor: 14);

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) }; // ~30 fps idle spin
        _timer.Tick += OnTick;

        Loaded += (_, _) => _timer.Start();
        Unloaded += (_, _) => { _timer.Stop(); _timer.Tick -= OnTick; };
    }

    private void OnTick(object? sender, object e)
    {
        _azimuth += 0.004f;                 // slow continuous orbit
        if (_azimuth > MathF.PI) _azimuth -= MathF.PI * 2f;
        Invalidate();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // Fill the host cell; fall back to a sensible size if measured unconstrained.
        var w = double.IsInfinity(availableSize.Width) ? 480 : availableSize.Width;
        var h = double.IsInfinity(availableSize.Height) ? 640 : availableSize.Height;
        return new Size(w, h);
    }

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        if (!IsSupportedOnCurrentPlatform()) return;

        var cx = (float)area.Width / 2f;
        var cy = (float)area.Height / 2f;
        var scale = (float)area.Height;     // tower (~0.8 of model height) fills most of the height

        var cosA = MathF.Cos(_azimuth);
        var sinA = MathF.Sin(_azimuth);
        var cosE = MathF.Cos(Elevation);
        var sinE = MathF.Sin(Elevation);

        SKPoint Project(Vec3 p)
        {
            // rotate around Y (orbit)
            var x1 = p.X * cosA + p.Z * sinA;
            var z1 = -p.X * sinA + p.Z * cosA;
            var y1 = p.Y;
            // tilt around X (elevation)
            var y2 = y1 * cosE - z1 * sinE;
            var z2 = y1 * sinE + z1 * cosE;
            var denom = z2 + CamDistance;
            return new SKPoint(cx + scale * x1 / denom, cy - scale * y2 / denom);
        }

        using var bodyPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.4f,
            Color = BodyBlue.WithAlpha(0xCC),
        };
        using var bodyDimPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.0f,
            Color = BodyBlueDim.WithAlpha(0x99),
        };
        using var glowPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 6f,
            Color = GlowGreen.WithAlpha(0x55),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6f),
        };
        using var glowCorePaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f,
            Color = GlowGreen,
        };

        // Back-to-front by floor isn't needed for wireframe; draw bottom-up.
        foreach (var floor in _floors)
        {
            var pts = new SKPoint[8];
            for (var i = 0; i < 8; i++) pts[i] = Project(floor.Corners[i]);

            if (floor.IsFlagged)
            {
                foreach (var (a, b) in FloorBox.Edges) canvas.DrawLine(pts[a], pts[b], glowPaint);
                foreach (var (a, b) in FloorBox.Edges) canvas.DrawLine(pts[a], pts[b], glowCorePaint);
            }
            else
            {
                // Verticals dimmer than the floor rings for a touch of depth.
                for (var e = 0; e < FloorBox.Edges.Length; e++)
                {
                    var (a, b) = FloorBox.Edges[e];
                    canvas.DrawLine(pts[a], pts[b], e >= 8 ? bodyDimPaint : bodyPaint);
                }
            }
        }
    }
}
