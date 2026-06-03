using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using SmartCity.Models;
using SmartCity.Services;

namespace SmartCity.Presentation.Controls;

/// <summary>
/// GPU-backed building twin (M3). Renders a Nakatomi-Plaza-style tower on the Uno Skia renderer
/// (<see cref="SKCanvasElement"/>): a terraced glass shaft drawn as depth-sorted 30%-opacity faces
/// with vertical mullions and lit windows, on top of which a wireframe reads the structure.
///
/// As a sample, the twin simulates an operations sweep: the "active" office floor advances on a slow
/// cadence and lights up; the rest of the tower twinkles. <see cref="ActiveFloorIndex"/> is a two-way
/// dependency property so the left column populates from the same source of truth. Floor 14F (the
/// AI-suggestion target) keeps a persistent green glow that pulses.
/// </summary>
public sealed partial class BuildingTwinView : SKCanvasElement
{
    // ── Tower / data-floor mapping ────────────────────────────────────────────────────────────
    // 18 storeys give the tower a skyscraper silhouette; the 7 service "data floors" (17F..11F)
    // occupy the office band just below the terraced crown. Active index 0 == 17F == top of band.
    private const int TotalStoreys = 18;
    private const int OfficeTopStorey = 14;  // storey index that data-floor index 0 (17F) maps to
    private const int DataFloorCount = 7;
    private const int FlaggedDataIndex = 3;  // 14F is the flagged optimization target
    private const int WindowsPerFace = 6;

    // Accent colours mirror the named theme brushes (TwinBodyBlue / TwinGlowGreen / AccentTeal).
    // SkiaSharp draws in SKColor, so they live here as constants resolved at draw time.
    private static readonly SKColor BodyBlue = new(0x5B, 0x82, 0xFF);
    private static readonly SKColor BodyBlueDim = new(0x3A, 0x52, 0xB0);
    private static readonly SKColor GlowGreen = new(0x46, 0xE0, 0x8C);
    private static readonly SKColor LiveCyan = new(0x9E, 0xFF, 0xFF);
    private static readonly SKColor WindowWarm = new(0xCF, 0xDD, 0xFF);

    private readonly IReadOnlyList<FloorBox> _storeys;
    private readonly float[][] _winPhase;     // per storey, per window: twinkle phase
    private readonly float[][] _winOccupancy; // per storey, per window: 0 = dark, else baseline level
    private readonly DispatcherTimer _timer;

    private float _t;                  // seconds since load (drives orbit + twinkle)
    private float _lastAdvance;        // last time the active floor advanced
    private const float AdvanceEvery = 2.0f;

    // Camera: orbit azimuth (slow idle spin) + fixed elevation, simple perspective.
    private float _azimuth = -0.62f;       // ~ -35° three-quarter view, matches the reference
    private const float Elevation = 0.28f; // ~ 16° downward tilt
    private const float CamDistance = 7.0f;

    public static readonly DependencyProperty ActiveFloorIndexProperty =
        DependencyProperty.Register(
            nameof(ActiveFloorIndex), typeof(int), typeof(BuildingTwinView),
            new PropertyMetadata(0, static (d, _) => ((BuildingTwinView)d).Invalidate()));

    /// <summary>Index (0..6) of the office floor currently lit; two-way bound to the model's state.</summary>
    public int ActiveFloorIndex
    {
        get => (int)GetValue(ActiveFloorIndexProperty);
        set => SetValue(ActiveFloorIndexProperty, value);
    }

    public BuildingTwinView()
    {
        _storeys = new BoxStackGeometryProvider().BuildTower(TotalStoreys, flaggedFloor: -1);

        // Seed a stable but scattered window pattern so the tower reads as a lived-in office block.
        var rng = new Random(1988); // Nakatomi Plaza, Christmas 1988
        _winPhase = new float[TotalStoreys][];
        _winOccupancy = new float[TotalStoreys][];
        for (var s = 0; s < TotalStoreys; s++)
        {
            _winPhase[s] = new float[WindowsPerFace];
            _winOccupancy[s] = new float[WindowsPerFace];
            for (var w = 0; w < WindowsPerFace; w++)
            {
                _winPhase[s][w] = (float)(rng.NextDouble() * Math.PI * 2);
                // ~55% of windows are "occupied" and glow; the rest stay dark.
                _winOccupancy[s][w] = rng.NextDouble() < 0.55 ? 0.35f + (float)rng.NextDouble() * 0.4f : 0f;
            }
        }

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) }; // ~30 fps
        _timer.Tick += OnTick;

        Loaded += (_, _) => _timer.Start();
        Unloaded += (_, _) => { _timer.Stop(); _timer.Tick -= OnTick; };
    }

    private void OnTick(object? sender, object e)
    {
        _t += 0.033f;
        _azimuth += 0.0022f;
        if (_azimuth > MathF.PI) _azimuth -= MathF.PI * 2f;

        // Advance the simulated active floor on a slow cadence; two-way binding pushes it to the model.
        if (_t - _lastAdvance >= AdvanceEvery)
        {
            _lastAdvance = _t;
            ActiveFloorIndex = (ActiveFloorIndex + 1) % DataFloorCount;
        }

        Invalidate();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var w = double.IsInfinity(availableSize.Width) ? 480 : availableSize.Width;
        var h = double.IsInfinity(availableSize.Height) ? 640 : availableSize.Height;
        return new Size(w, h);
    }

    private readonly record struct Face(int Storey, int Kind, SKPoint P0, SKPoint P1, SKPoint P2, SKPoint P3, float Depth);

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        if (!IsSupportedOnCurrentPlatform()) return;

        var cx = (float)area.Width / 2f;
        var cy = (float)area.Height / 2f;
        var scale = (float)Math.Min(area.Width, area.Height) * 1.15f;

        var cosA = MathF.Cos(_azimuth);
        var sinA = MathF.Sin(_azimuth);
        var cosE = MathF.Cos(Elevation);
        var sinE = MathF.Sin(Elevation);

        (SKPoint pt, float depth) Project(Vec3 p)
        {
            var x1 = p.X * cosA + p.Z * sinA;
            var z1 = -p.X * sinA + p.Z * cosA;
            var y1 = p.Y;
            var y2 = y1 * cosE - z1 * sinE;
            var z2 = y1 * sinE + z1 * cosE;
            var denom = z2 + CamDistance;
            return (new SKPoint(cx + scale * x1 / denom, cy - scale * y2 / denom), z2);
        }

        var activeStorey = OfficeTopStorey - Math.Clamp(ActiveFloorIndex, 0, DataFloorCount - 1);
        var flaggedStorey = OfficeTopStorey - FlaggedDataIndex;

        // Collect every visible face with its view-space depth, then painter's-sort back-to-front.
        var faces = new List<Face>(TotalStoreys * 5);
        var pts = new SKPoint[8];
        var depth = new float[8];
        foreach (var storey in _storeys)
        {
            for (var i = 0; i < 8; i++) (pts[i], depth[i]) = Project(storey.Corners[i]);
            var s = storey.Floor - 1;
            for (var k = 0; k < FloorBox.Faces.Length; k++)
            {
                var (a, b, c, d) = FloorBox.Faces[k];
                var avg = (depth[a] + depth[b] + depth[c] + depth[d]) / 4f;
                faces.Add(new Face(s, k, pts[a], pts[b], pts[c], pts[d], avg));
            }
        }
        faces.Sort(static (l, r) => r.Depth.CompareTo(l.Depth));

        using var fill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var stroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.1f };
        using var mullion = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 0.7f, Color = BodyBlueDim.WithAlpha(0x55) };
        using var glow = new SKPaint
        {
            IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 6f,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6f),
        };

        var pulse = 0.5f + 0.5f * MathF.Sin(_t * 2.4f); // shared pulse for active / flagged accents

        foreach (var f in faces)
        {
            var isActive = f.Storey == activeStorey;
            var isFlagged = f.Storey == flaggedStorey;
            var isTop = f.Kind == 4;

            using var path = new SKPath();
            path.MoveTo(f.P0); path.LineTo(f.P1); path.LineTo(f.P2); path.LineTo(f.P3); path.Close();

            // ── Glass fill (30% body blue baseline; brighter when lit) ──
            if (isFlagged)
                fill.Color = GlowGreen.WithAlpha((byte)(0x40 + 0x40 * pulse));
            else if (isActive)
                fill.Color = LiveCyan.WithAlpha((byte)(0x38 + 0x30 * pulse));
            else
                fill.Color = BodyBlue.WithAlpha(isTop ? (byte)0x33 : (byte)0x48); // ~30% glass
            canvas.DrawPath(path, fill);

            if (!isTop)
            {
                DrawWindows(canvas, fill, f, isActive, isFlagged, pulse);
                DrawMullions(canvas, mullion, f);
            }

            // ── Edge / outline ──
            if (isFlagged)
            {
                glow.Color = GlowGreen.WithAlpha((byte)(0x40 + 0x40 * pulse));
                canvas.DrawPath(path, glow);
                stroke.Color = GlowGreen;
            }
            else if (isActive)
            {
                glow.Color = LiveCyan.WithAlpha((byte)(0x30 + 0x30 * pulse));
                canvas.DrawPath(path, glow);
                stroke.Color = LiveCyan.WithAlpha(0xDD);
            }
            else
            {
                stroke.Color = BodyBlue.WithAlpha(isTop ? (byte)0x66 : (byte)0xAA);
            }
            canvas.DrawPath(path, stroke);
        }
    }

    /// <summary>Draws the lit-window row for one storey face via bilinear interpolation across the quad.</summary>
    private void DrawWindows(SKCanvas canvas, SKPaint fill, Face f, bool isActive, bool isFlagged, float pulse)
    {
        // Quad order: P0 bottom-left, P1 bottom-right, P2 top-right, P3 top-left.
        const float inset = 0.16f, gap = 0.5f / WindowsPerFace;
        for (var w = 0; w < WindowsPerFace; w++)
        {
            var occ = _winOccupancy[f.Storey][w];

            float level;
            SKColor baseColor;
            if (isActive) { level = 0.85f + 0.15f * pulse; baseColor = LiveCyan; }
            else if (isFlagged) { level = 0.7f + 0.3f * pulse; baseColor = GlowGreen; }
            else
            {
                if (occ <= 0f) continue; // dark window
                var twinkle = 0.5f + 0.5f * MathF.Sin(_t * 1.3f + _winPhase[f.Storey][w]);
                level = occ * (0.45f + 0.55f * twinkle);
                baseColor = WindowWarm;
            }

            var u0 = w / (float)WindowsPerFace + gap * 0.5f;
            var u1 = (w + 1) / (float)WindowsPerFace - gap * 0.5f;
            var p = QuadCell(f, u0, u1, inset, 1f - inset);

            fill.Color = baseColor.WithAlpha((byte)Math.Clamp(level * 255f, 0, 255));
            using var win = new SKPath();
            win.MoveTo(p.A); win.LineTo(p.B); win.LineTo(p.C); win.LineTo(p.D); win.Close();
            canvas.DrawPath(win, fill);
        }
    }

    private static void DrawMullions(SKCanvas canvas, SKPaint mullion, Face f)
    {
        for (var m = 1; m < WindowsPerFace; m++)
        {
            var u = m / (float)WindowsPerFace;
            var bottom = Lerp(f.P0, f.P1, u);
            var top = Lerp(f.P3, f.P2, u);
            canvas.DrawLine(bottom, top, mullion);
        }
    }

    private static (SKPoint A, SKPoint B, SKPoint C, SKPoint D) QuadCell(Face f, float u0, float u1, float v0, float v1)
    {
        SKPoint At(float u, float v)
        {
            var b = Lerp(f.P0, f.P1, u); // bottom edge
            var t = Lerp(f.P3, f.P2, u); // top edge
            return Lerp(b, t, v);
        }
        return (At(u0, v0), At(u1, v0), At(u1, v1), At(u0, v1));
    }

    private static SKPoint Lerp(SKPoint a, SKPoint b, float t) =>
        new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
}
