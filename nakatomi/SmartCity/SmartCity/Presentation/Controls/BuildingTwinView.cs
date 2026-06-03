using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using SmartCity.Models;
using SmartCity.Services;

namespace SmartCity.Presentation.Controls;

/// <summary>
/// GPU-backed building twin (M3/M4). Renders a Nakatomi-Plaza-style glass tower on the Uno Skia
/// renderer (<see cref="SKCanvasElement"/>): depth-sorted ~30%-opacity glass faces with a lit window
/// grid over a wireframe. Floor 14F (the AI-suggestion target) wears a persistent glowing green
/// optimization band; the office band quietly twinkles and a soft highlight sweeps the floors.
///
/// Interaction (M4): pointer-drag orbits, the wheel zooms, <see cref="Toggle2D"/> tips to a top-down
/// plan, and <see cref="TogglePause"/> freezes the simulation (also serving reduced-motion).
/// <see cref="ActiveFloorIndex"/> is a two-way DP so the floor ruler + tooltip stay in lockstep.
/// </summary>
public sealed partial class BuildingTwinView : SKCanvasElement
{
    // ── Tower / data-floor mapping ────────────────────────────────────────────────────────────
    private const int TotalStoreys = 18;
    private const int OfficeTopStorey = 14;  // storey index that data-floor index 0 (17F) maps to
    private const int DataFloorCount = 7;
    private const int FlaggedDataIndex = 3;  // 14F is the flagged optimization target
    private const int WindowsPerFace = 8;
    private const int WindowRowsPerStorey = 2;

    private static readonly SKColor BodyBlue = new(0x6E, 0x93, 0xFF);
    private static readonly SKColor BodyBlueDim = new(0x35, 0x49, 0x8F);
    private static readonly SKColor GlowGreen = new(0x46, 0xE0, 0x8C);
    private static readonly SKColor LiveCyan = new(0x9E, 0xFF, 0xFF);
    private static readonly SKColor WindowWarm = new(0xD7, 0xE4, 0xFF);

    private readonly IReadOnlyList<FloorBox> _storeys;
    private readonly float[][] _winPhase;
    private readonly float[][] _winOccupancy;
    private readonly DispatcherTimer _timer;

    private float _t;
    private float _lastAdvance;
    private const float AdvanceEvery = 2.0f;

    // Camera (mutable for interaction).
    private float _azimuth = -0.62f;
    private float _elevation = 0.46f;       // ~26° down — reveals the ground plane + cityscape
    private float _targetElevation = 0.46f;
    private float _camDistance = 6.9f;
    private bool _paused;
    private bool _is2D;
    private bool _dragging;
    private Point _lastPoint;

    // Cityscape backdrop + elevators.
    private readonly List<CityBox> _city = new();
    private float _groundY;
    private static readonly SKColor ElevatorAmber = new(0xFF, 0xB4, 0x4C);
    private static readonly SKColor CityLight = new(0x3E, 0x53, 0x84);
    private static readonly SKColor CityDark = new(0x12, 0x1B, 0x30);
    private const int ElevatorShafts = 3;

    private readonly record struct CityBox(Vec3[] Corners, float Tone, float SpeedSeed);

    public bool IsPaused => _paused;
    public bool Is2D => _is2D;

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

        var rng = new Random(1988); // Nakatomi Plaza, Christmas 1988
        _winPhase = new float[TotalStoreys][];
        _winOccupancy = new float[TotalStoreys][];
        var cells = WindowsPerFace * WindowRowsPerStorey;
        for (var s = 0; s < TotalStoreys; s++)
        {
            _winPhase[s] = new float[cells];
            _winOccupancy[s] = new float[cells];
            for (var c = 0; c < cells; c++)
            {
                _winPhase[s][c] = (float)(rng.NextDouble() * Math.PI * 2);
                _winOccupancy[s][c] = rng.NextDouble() < 0.6 ? 0.4f + (float)rng.NextDouble() * 0.4f : 0f;
            }
        }

        // Neumorphic cityscape: scattered extruded blocks ringing the tower on the ground plane.
        _groundY = _storeys[0].Corners[0].Y;
        var crng = new Random(7);
        for (var n = 0; n < 44; n++)
        {
            var ang = (float)(crng.NextDouble() * Math.PI * 2);
            var rad = 1.7f + (float)crng.NextDouble() * 3.6f;
            var bx = MathF.Cos(ang) * rad;
            var bz = MathF.Sin(ang) * rad;
            var hw = 0.22f + (float)crng.NextDouble() * 0.34f;
            var hd = 0.22f + (float)crng.NextDouble() * 0.34f;
            var h = 0.35f + (float)crng.NextDouble() * 1.9f;
            var y0 = _groundY;
            var y1 = _groundY + h;
            var c = new[]
            {
                new Vec3(bx - hw, y0, bz - hd), new Vec3(bx + hw, y0, bz - hd), new Vec3(bx + hw, y0, bz + hd), new Vec3(bx - hw, y0, bz + hd),
                new Vec3(bx - hw, y1, bz - hd), new Vec3(bx + hw, y1, bz - hd), new Vec3(bx + hw, y1, bz + hd), new Vec3(bx - hw, y1, bz + hd),
            };
            _city.Add(new CityBox(c, 0.65f + (float)crng.NextDouble() * 0.7f, (float)crng.NextDouble()));
        }

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
        _timer.Tick += OnTick;

        PointerPressed += OnPointerPressedH;
        PointerMoved += OnPointerMovedH;
        PointerReleased += OnPointerReleasedH;
        PointerWheelChanged += OnPointerWheelH;

        Loaded += (_, _) => _timer.Start();
        Unloaded += (_, _) =>
        {
            _timer.Stop();
            _timer.Tick -= OnTick;
            PointerPressed -= OnPointerPressedH;
            PointerMoved -= OnPointerMovedH;
            PointerReleased -= OnPointerReleasedH;
            PointerWheelChanged -= OnPointerWheelH;
        };
    }

    // ── Interaction ───────────────────────────────────────────────────────────────────────────
    public void ZoomIn() { _camDistance = Math.Clamp(_camDistance - 0.6f, 3.8f, 11f); Invalidate(); }
    public void ZoomOut() { _camDistance = Math.Clamp(_camDistance + 0.6f, 3.8f, 11f); Invalidate(); }
    public void Toggle2D() { _is2D = !_is2D; _targetElevation = _is2D ? 1.45f : 0.30f; if (_paused) Invalidate(); }
    public void TogglePause() { _paused = !_paused; if (!_paused) _timer.Start(); else _timer.Stop(); }
    public void Pause() { _paused = true; _timer.Stop(); }

    private void OnPointerPressedH(object sender, PointerRoutedEventArgs e)
    {
        _dragging = true;
        _lastPoint = e.GetCurrentPoint(this).Position;
        CapturePointer(e.Pointer);
    }

    private void OnPointerMovedH(object sender, PointerRoutedEventArgs e)
    {
        if (!_dragging) return;
        var p = e.GetCurrentPoint(this).Position;
        _azimuth += (float)(p.X - _lastPoint.X) * 0.01f;
        _targetElevation = Math.Clamp(_targetElevation - (float)(p.Y - _lastPoint.Y) * 0.006f, -0.25f, 1.5f);
        _is2D = _targetElevation > 1.1f;
        _lastPoint = p;
        if (_paused) { _elevation = _targetElevation; Invalidate(); }
    }

    private void OnPointerReleasedH(object sender, PointerRoutedEventArgs e)
    {
        _dragging = false;
        ReleasePointerCapture(e.Pointer);
    }

    private void OnPointerWheelH(object sender, PointerRoutedEventArgs e)
    {
        var delta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;
        _camDistance = Math.Clamp(_camDistance - delta * 0.0016f, 3.8f, 11f);
        if (_paused) Invalidate();
    }

    private void OnTick(object? sender, object e)
    {
        _t += 0.033f;
        if (!_dragging && !_is2D) _azimuth += 0.0022f;
        if (_azimuth > MathF.PI) _azimuth -= MathF.PI * 2f;

        _elevation += (_targetElevation - _elevation) * 0.12f; // ease toward 2D / 3D

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
        var scale = (float)Math.Min(area.Width, area.Height) * 1.2f;

        var cosA = MathF.Cos(_azimuth);
        var sinA = MathF.Sin(_azimuth);
        var cosE = MathF.Cos(_elevation);
        var sinE = MathF.Sin(_elevation);

        (SKPoint pt, float depth) Project(Vec3 p)
        {
            var x1 = p.X * cosA + p.Z * sinA;
            var z1 = -p.X * sinA + p.Z * cosA;
            var y1 = p.Y;
            var y2 = y1 * cosE - z1 * sinE;
            var z2 = y1 * sinE + z1 * cosE;
            var denom = z2 + _camDistance;
            return (new SKPoint(cx + scale * x1 / denom, cy - scale * y2 / denom), z2);
        }

        // Backdrop first: ground glow + neumorphic city blocks behind the tower.
        DrawGround(canvas, Project);
        DrawCity(canvas, Project);

        var activeStorey = OfficeTopStorey - Math.Clamp(ActiveFloorIndex, 0, DataFloorCount - 1);
        var flaggedStorey = OfficeTopStorey - FlaggedDataIndex;

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
        using var mullion = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 0.7f, Color = BodyBlueDim.WithAlpha(0x60) };
        using var glow = new SKPaint
        {
            IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 9f,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 9f),
        };

        var pulse = 0.5f + 0.5f * MathF.Sin(_t * 2.2f);

        foreach (var f in faces)
        {
            var isActive = f.Storey == activeStorey;
            var isFlagged = f.Storey == flaggedStorey;
            var isTop = f.Kind == 4;

            using var path = new SKPath();
            path.MoveTo(f.P0); path.LineTo(f.P1); path.LineTo(f.P2); path.LineTo(f.P3); path.Close();

            if (isFlagged)
                fill.Color = GlowGreen.WithAlpha((byte)(0x55 + 0x30 * pulse)); // green optimization band
            else if (isActive)
                fill.Color = LiveCyan.WithAlpha((byte)(0x34 + 0x24 * pulse));
            else
                fill.Color = BodyBlue.WithAlpha(isTop ? (byte)0x38 : (byte)0x4E);
            canvas.DrawPath(path, fill);

            if (!isTop)
            {
                DrawWindows(canvas, fill, f, isActive, isFlagged, pulse);
                DrawMullions(canvas, mullion, f);
            }

            if (isFlagged)
            {
                glow.Color = GlowGreen.WithAlpha((byte)(0x66 + 0x40 * pulse));
                canvas.DrawPath(path, glow);
                stroke.Color = GlowGreen;
                stroke.StrokeWidth = 1.6f;
            }
            else if (isActive)
            {
                glow.Color = LiveCyan.WithAlpha((byte)(0x2A + 0x26 * pulse));
                canvas.DrawPath(path, glow);
                stroke.Color = LiveCyan.WithAlpha(0xDD);
                stroke.StrokeWidth = 1.2f;
            }
            else
            {
                stroke.Color = BodyBlue.WithAlpha(isTop ? (byte)0x70 : (byte)0xB0);
                stroke.StrokeWidth = 1.0f;
            }
            canvas.DrawPath(path, stroke);
        }

        // Elevators ride the core in front of the glass, in amber (distinct from the floor green).
        DrawElevators(canvas, Project);
    }

    // ── Cityscape backdrop ──────────────────────────────────────────────────────────────────────
    private void DrawGround(SKCanvas canvas, Func<Vec3, (SKPoint pt, float depth)> project)
    {
        const float r = 6.0f;
        var bl = project(new Vec3(-r, _groundY, -r)).pt;
        var br = project(new Vec3(r, _groundY, -r)).pt;
        var fr = project(new Vec3(r, _groundY, r)).pt;
        var fl = project(new Vec3(-r, _groundY, r)).pt;
        var center = project(new Vec3(0, _groundY, 0)).pt;
        var radius = Math.Max(Dist(center, bl), Dist(center, fr));

        using var path = new SKPath();
        path.MoveTo(bl); path.LineTo(br); path.LineTo(fr); path.LineTo(fl); path.Close();
        using var fill = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Shader = SKShader.CreateRadialGradient(
                center, radius,
                new[] { new SKColor(0x1B, 0x29, 0x46), new SKColor(0x0B, 0x10, 0x1C), new SKColor(0x08, 0x0B, 0x12) },
                new[] { 0f, 0.6f, 1f }, SKShaderTileMode.Clamp),
        };
        canvas.DrawPath(path, fill);
    }

    private void DrawCity(SKCanvas canvas, Func<Vec3, (SKPoint pt, float depth)> project)
    {
        var faces = new List<(SKPoint a, SKPoint b, SKPoint c, SKPoint d, float depth, bool top, float tone)>(_city.Count * 5);
        var p = new SKPoint[8];
        var z = new float[8];
        foreach (var box in _city)
        {
            for (var i = 0; i < 8; i++) (p[i], z[i]) = project(box.Corners[i]);
            for (var k = 0; k < FloorBox.Faces.Length; k++)
            {
                var (ia, ib, ic, id) = FloorBox.Faces[k];
                var avg = (z[ia] + z[ib] + z[ic] + z[id]) / 4f;
                faces.Add((p[ia], p[ib], p[ic], p[id], avg, k == 4, box.Tone));
            }
        }
        faces.Sort(static (l, r) => r.depth.CompareTo(l.depth));

        using var fill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var edge = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f };
        foreach (var f in faces)
        {
            using var path = new SKPath();
            path.MoveTo(f.a); path.LineTo(f.b); path.LineTo(f.c); path.LineTo(f.d); path.Close();

            if (f.top)
            {
                // Lit rooftop.
                fill.Shader = null;
                fill.Color = Lerp(CityDark, CityLight, Math.Clamp(f.tone, 0f, 1f));
                canvas.DrawPath(path, fill);
                // Bright top-edge highlight reads as the neumorphic catch-light.
                edge.Color = new SKColor(0x6A, 0x84, 0xC4).WithAlpha(0xC0);
                canvas.DrawPath(path, edge);
            }
            else
            {
                // Side wall: vertical gradient from a lit top edge down into soft shadow.
                var topMid = Lerp(f.c, f.d, 0.5f);
                var botMid = Lerp(f.a, f.b, 0.5f);
                var topCol = Lerp(CityDark, CityLight, Math.Clamp(0.85f * f.tone, 0f, 1f));
                var botCol = Lerp(CityDark, CityLight, 0.12f);
                fill.Shader = SKShader.CreateLinearGradient(
                    topMid, botMid, new[] { topCol, botCol }, new[] { 0f, 1f }, SKShaderTileMode.Clamp);
                canvas.DrawPath(path, fill);
                fill.Shader = null;
            }
        }
    }

    private static float Dist(SKPoint a, SKPoint b) => MathF.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

    // ── Elevators ───────────────────────────────────────────────────────────────────────────────
    private void DrawElevators(SKCanvas canvas, Func<Vec3, (SKPoint pt, float depth)> project)
    {
        var bottom = _storeys[0].Corners;
        var top = _storeys[^1].Corners;

        // The four tower sides as full-height quads (bottom-left, bottom-right, top-right, top-left).
        Span<(int b0, int b1, int t1, int t0)> sides =
        [
            (0, 1, 5, 4), (1, 2, 6, 5), (2, 3, 7, 6), (3, 0, 4, 7),
        ];

        var bestSide = -1;
        var bestDepth = float.MaxValue;
        var quad = new SKPoint[4];
        for (var s = 0; s < 4; s++)
        {
            var (b0, b1, t1, t0) = sides[s];
            var (q0, d0) = project(bottom[b0]);
            var (q1, d1) = project(bottom[b1]);
            var (q2, d2) = project(top[t1]);
            var (q3, d3) = project(top[t0]);
            var avg = (d0 + d1 + d2 + d3) / 4f;
            if (avg < bestDepth) { bestDepth = avg; bestSide = s; quad[0] = q0; quad[1] = q1; quad[2] = q2; quad[3] = q3; }
        }
        if (bestSide < 0) return;

        SKPoint Q(float u, float v) => Lerp(Lerp(quad[0], quad[1], u), Lerp(quad[3], quad[2], u), v);

        using var shaft = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = ElevatorAmber.WithAlpha(0x26) };
        using var rail = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f, Color = ElevatorAmber.WithAlpha(0x55) };
        using var car = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var glow = new SKPaint
        {
            IsAntialias = true, Style = SKPaintStyle.Fill, Color = ElevatorAmber.WithAlpha(0x66),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6f),
        };

        const float vTop = 0.06f, vBot = 0.94f, carH = 0.05f, halfW = 0.022f;
        for (var e = 0; e < ElevatorShafts; e++)
        {
            var u = 0.30f + e * (0.40f / Math.Max(1, ElevatorShafts - 1)); // cluster the shafts near the core

            // Shaft channel: a translucent fill of the elevator colour itself.
            using var shaftPath = new SKPath();
            shaftPath.MoveTo(Q(u - halfW, vTop)); shaftPath.LineTo(Q(u + halfW, vTop));
            shaftPath.LineTo(Q(u + halfW, vBot)); shaftPath.LineTo(Q(u - halfW, vBot)); shaftPath.Close();
            canvas.DrawPath(shaftPath, shaft);

            // Shaft rails.
            canvas.DrawLine(Q(u - halfW, vTop), Q(u - halfW, vBot), rail);
            canvas.DrawLine(Q(u + halfW, vTop), Q(u + halfW, vBot), rail);

            // Car position oscillates up/down; alternate directions and speeds per shaft.
            var dir = e % 2 == 0 ? 1f : -1f;
            var speed = 0.6f + e * 0.25f;
            var vc = vTop + (vBot - vTop) * (0.5f + 0.5f * MathF.Sin(_t * speed * dir + e * 1.7f));

            var c0 = Q(u - halfW, vc - carH * 0.5f);
            var c1 = Q(u + halfW, vc - carH * 0.5f);
            var c2 = Q(u + halfW, vc + carH * 0.5f);
            var c3 = Q(u - halfW, vc + carH * 0.5f);
            using var path = new SKPath();
            path.MoveTo(c0); path.LineTo(c1); path.LineTo(c2); path.LineTo(c3); path.Close();
            canvas.DrawPath(path, glow);
            car.Color = ElevatorAmber;
            canvas.DrawPath(path, car);
        }
    }

    private void DrawWindows(SKCanvas canvas, SKPaint fill, Face f, bool isActive, bool isFlagged, float pulse)
    {
        const float gap = 0.42f / WindowsPerFace;
        for (var row = 0; row < WindowRowsPerStorey; row++)
        {
            var v0 = 0.16f + row * (0.84f / WindowRowsPerStorey);
            var v1 = v0 + (0.84f / WindowRowsPerStorey) - 0.10f;
            for (var w = 0; w < WindowsPerFace; w++)
            {
                var cell = row * WindowsPerFace + w;
                var occ = _winOccupancy[f.Storey][cell];

                float level;
                SKColor baseColor;
                if (isFlagged) { level = 0.85f + 0.15f * pulse; baseColor = GlowGreen; }
                else if (isActive) { level = 0.8f + 0.2f * pulse; baseColor = LiveCyan; }
                else
                {
                    if (occ <= 0f) continue;
                    var twinkle = 0.5f + 0.5f * MathF.Sin(_t * 1.3f + _winPhase[f.Storey][cell]);
                    level = occ * (0.5f + 0.5f * twinkle);
                    baseColor = WindowWarm;
                }

                var u0 = w / (float)WindowsPerFace + gap * 0.5f;
                var u1 = (w + 1) / (float)WindowsPerFace - gap * 0.5f;
                var p = QuadCell(f, u0, u1, v0, v1);

                fill.Color = baseColor.WithAlpha((byte)Math.Clamp(level * 255f, 0, 255));
                using var win = new SKPath();
                win.MoveTo(p.A); win.LineTo(p.B); win.LineTo(p.C); win.LineTo(p.D); win.Close();
                canvas.DrawPath(win, fill);
            }
        }
    }

    private static void DrawMullions(SKCanvas canvas, SKPaint mullion, Face f)
    {
        for (var m = 1; m < WindowsPerFace; m++)
        {
            var u = m / (float)WindowsPerFace;
            canvas.DrawLine(Lerp(f.P0, f.P1, u), Lerp(f.P3, f.P2, u), mullion);
        }
    }

    private static (SKPoint A, SKPoint B, SKPoint C, SKPoint D) QuadCell(Face f, float u0, float u1, float v0, float v1)
    {
        SKPoint At(float u, float v) => Lerp(Lerp(f.P0, f.P1, u), Lerp(f.P3, f.P2, u), v);
        return (At(u0, v0), At(u1, v0), At(u1, v1), At(u0, v1));
    }

    private static SKPoint Lerp(SKPoint a, SKPoint b, float t) =>
        new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);

    private static SKColor Lerp(SKColor a, SKColor b, float t) => new(
        (byte)(a.Red + (b.Red - a.Red) * t),
        (byte)(a.Green + (b.Green - a.Green) * t),
        (byte)(a.Blue + (b.Blue - a.Blue) * t));
}
