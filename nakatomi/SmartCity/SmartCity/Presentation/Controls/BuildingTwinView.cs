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
/// renderer (<see cref="SKCanvasElement"/>): depth-sorted glass faces with a lit window grid over a
/// wireframe, on a neumorphic cityscape, with amber elevators. Floor 14F wears a persistent green
/// optimization band; the office band twinkles and a highlight sweeps the floors.
///
/// Interaction: pointer-drag orbits, the wheel zooms, <see cref="Toggle2D"/> tips to top-down, and
/// <see cref="TogglePause"/> freezes the simulation. <see cref="ActiveFloorIndex"/> is a two-way DP.
///
/// Perf (Phase 1): all SkiaSharp paints/filters and two scratch <see cref="SKPath"/>s are allocated
/// once and reused; geometry buffers and the face list are reused per frame; back-facing side faces
/// are culled; the render loop pauses when the window deactivates. The render path makes ~no per-frame
/// managed/native allocations.
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

    private static readonly SKColor BodyBlue = new(0x83, 0x8E, 0xFF);     // vivid blue-violet glass
    private static readonly SKColor BodyBlueDim = new(0x3C, 0x44, 0x96);
    private static readonly SKColor GlowGreen = new(0x46, 0xE0, 0x8C);
    private static readonly SKColor LiveCyan = new(0x9E, 0xFF, 0xFF);
    private static readonly SKColor WindowWarm = new(0xD7, 0xE4, 0xFF);
    private static readonly SKColor ElevatorAmber = new(0xFF, 0xB4, 0x4C);
    private static readonly SKColor CityLight = new(0x3E, 0x53, 0x84);
    private static readonly SKColor CityDark = new(0x12, 0x1B, 0x30);
    private const int ElevatorShafts = 3;

    private readonly IReadOnlyList<FloorBox> _storeys;
    private readonly IReadOnlyList<FloorBox> _tower2;
    private readonly float[][] _winPhase;
    private readonly float[][] _winOccupancy;
    private readonly DispatcherTimer _timer;

    // Second (smaller) building + the energy link between the two towers.
    private static readonly SKColor Tower2Glass = new(0x4F, 0xC8, 0xD8);
    private static readonly SKColor EnergyCyan = new(0x6C, 0xF2, 0xFF);
    private static readonly SKColor EnergyMint = new(0x7C, 0xFF, 0xC8);
    private const int Tower2Floors = 9;
    private const float Tower2OffX = 2.6f;
    private const float Tower2OffZ = -0.5f;
    private const float Tower2Scale = 0.60f;
    private const float FloorHeightModel = 0.30f; // mirrors BoxStackGeometryProvider

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
    private bool _windowInactive;
    private Point _lastPoint;

    // Cityscape backdrop.
    private readonly List<CityBox> _city = new();
    private readonly List<Vec3[]> _roads = new();
    private float _groundY;
    private float _anchorY;        // screen Y of the active floor (for the synced tooltip)
    private int _lastElevFloor = -1;

    // ── Reused render objects (Phase 1: no per-frame allocations) ───────────────────────────────
    private readonly SKPaint _fill = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _stroke = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.1f };
    private readonly SKPaint _mullion = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 0.7f, Color = BodyBlueDim.WithAlpha(0x60) };
    private readonly SKPaint _glow = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 9f, MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 9f) };
    private readonly SKPaint _cityFill = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _cityEdge = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f, Color = new SKColor(0x6A, 0x84, 0xC4).WithAlpha(0xC0) };
    private readonly SKPaint _groundPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _shaftPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill, Color = ElevatorAmber.WithAlpha(0x26) };
    private readonly SKPaint _railPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f, Color = ElevatorAmber.WithAlpha(0x55) };
    private readonly SKPaint _carPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill, Color = ElevatorAmber };
    private readonly SKPaint _carGlow = new() { IsAntialias = true, Style = SKPaintStyle.Fill, Color = ElevatorAmber.WithAlpha(0x66), MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6f) };
    private readonly SKPaint _roadPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2.2f, Color = new SKColor(0x33, 0x3F, 0x59).WithAlpha(0x9A) };
    private readonly SKPaint _arc = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.6f, StrokeCap = SKStrokeCap.Round, Color = EnergyCyan.WithAlpha(0xA0) };
    private readonly SKPaint _arcGlow = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 6f, Color = EnergyCyan.WithAlpha(0x3A), MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5f) };
    private readonly SKPaint _particle = new() { IsAntialias = true, Style = SKPaintStyle.Fill, Color = EnergyCyan };
    private readonly SKPaint _particleGlow = new() { IsAntialias = true, Style = SKPaintStyle.Fill, Color = EnergyCyan.WithAlpha(0x80), MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5f) };
    private readonly SKPath _facePath = new();
    private readonly SKPath _cellPath = new();
    private readonly List<Face> _faces = new(TotalStoreys * 5);
    private readonly List<CityFace> _cityFaces = new(64 * 5);
    private readonly SKPoint[] _proj = new SKPoint[8];
    private readonly float[] _projDepth = new float[8];
    private readonly SKPoint[] _quad = new SKPoint[4];

    private readonly record struct CityBox(Vec3[] Corners, float Tone);
    private readonly record struct CityFace(SKPoint A, SKPoint B, SKPoint C, SKPoint D, float Depth, bool Top, float Tone);

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

    // Live readouts the left column binds to (ElementName=Twin), kept in sync with the canvas each frame.
    public static readonly DependencyProperty ActiveAnchorYProperty =
        DependencyProperty.Register(nameof(ActiveAnchorY), typeof(double), typeof(BuildingTwinView), new PropertyMetadata(0.0));
    /// <summary>Screen-space Y of the highlighted floor — the tooltip tracks this so it stays beside the lit band.</summary>
    public double ActiveAnchorY { get => (double)GetValue(ActiveAnchorYProperty); private set => SetValue(ActiveAnchorYProperty, value); }

    public static readonly DependencyProperty ElevatorFloorTextProperty =
        DependencyProperty.Register(nameof(ElevatorFloorText), typeof(string), typeof(BuildingTwinView), new PropertyMetadata("—"));
    /// <summary>Lead elevator car's current floor, e.g. "12F" — synced to the moving car in the canvas.</summary>
    public string ElevatorFloorText { get => (string)GetValue(ElevatorFloorTextProperty); private set => SetValue(ElevatorFloorTextProperty, value); }

    public static readonly DependencyProperty ElevatorDirTextProperty =
        DependencyProperty.Register(nameof(ElevatorDirText), typeof(string), typeof(BuildingTwinView), new PropertyMetadata("—"));
    /// <summary>Lead elevator direction, e.g. "▲ Ascending".</summary>
    public string ElevatorDirText { get => (string)GetValue(ElevatorDirTextProperty); private set => SetValue(ElevatorDirTextProperty, value); }

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
            _city.Add(new CityBox(c, 0.65f + (float)crng.NextDouble() * 0.7f));
        }

        // Street grid on the ground (straight ground lines project to straight screen lines).
        for (var g = -1; g <= 1; g++)
        {
            var o = g * 2.6f;
            _roads.Add(new[] { new Vec3(o, _groundY, -5.4f), new Vec3(o, _groundY, 5.4f) });
            _roads.Add(new[] { new Vec3(-5.4f, _groundY, o), new Vec3(5.4f, _groundY, o) });
        }

        // Second, smaller tower: built by the same provider, then scaled in footprint and translated
        // to stand on the ground plane offset from the main tower.
        var t2 = new BoxStackGeometryProvider().BuildTower(Tower2Floors, flaggedFloor: -1);
        var translateY = _groundY + Tower2Floors * FloorHeightModel / 2f;
        var list2 = new List<FloorBox>(t2.Count);
        foreach (var fb in t2)
        {
            var nc = new Vec3[8];
            for (var i = 0; i < 8; i++)
                nc[i] = new Vec3(fb.Corners[i].X * Tower2Scale + Tower2OffX, fb.Corners[i].Y + translateY, fb.Corners[i].Z * Tower2Scale + Tower2OffZ);
            list2.Add(new FloorBox(fb.Floor, fb.Label, nc, false));
        }
        _tower2 = list2;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
        _timer.Tick += OnTick;

        PointerPressed += OnPointerPressedH;
        PointerMoved += OnPointerMovedH;
        PointerReleased += OnPointerReleasedH;
        PointerWheelChanged += OnPointerWheelH;

        Loaded += OnLoadedH;
        Unloaded += OnUnloadedH;
    }

    private void OnLoadedH(object sender, RoutedEventArgs e)
    {
        if (App.MainWindowInstance is { } win) win.Activated += OnWindowActivated;
        if (!_paused) _timer.Start();
    }

    private void OnUnloadedH(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        _timer.Tick -= OnTick;
        if (App.MainWindowInstance is { } win) win.Activated -= OnWindowActivated;
        PointerPressed -= OnPointerPressedH;
        PointerMoved -= OnPointerMovedH;
        PointerReleased -= OnPointerReleasedH;
        PointerWheelChanged -= OnPointerWheelH;
    }

    // Pause the render loop while the window is deactivated (idle CPU/GPU, reduced-motion friendly).
    private void OnWindowActivated(object sender, WindowActivatedEventArgs e)
    {
        _windowInactive = e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated;
        if (_windowInactive) _timer.Stop();
        else if (!_paused) _timer.Start();
    }

    // ── Interaction ───────────────────────────────────────────────────────────────────────────
    public void ZoomIn() { _camDistance = Math.Clamp(_camDistance - 0.6f, 3.8f, 11f); Invalidate(); }
    public void ZoomOut() { _camDistance = Math.Clamp(_camDistance + 0.6f, 3.8f, 11f); Invalidate(); }
    public void Toggle2D() { _is2D = !_is2D; _targetElevation = _is2D ? 1.45f : 0.30f; if (_paused) Invalidate(); }
    public void TogglePause() { _paused = !_paused; if (!_paused && !_windowInactive) _timer.Start(); else _timer.Stop(); }
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

    private readonly record struct Face(int Storey, int Kind, SKPoint P0, SKPoint P1, SKPoint P2, SKPoint P3, float Depth, int Building);

    // Camera state captured per frame so Project can stay a cheap static-like local.
    private float _cx, _cy, _scale, _cosA, _sinA, _cosE, _sinE;

    private (SKPoint pt, float depth) Project(Vec3 p)
    {
        var x1 = p.X * _cosA + p.Z * _sinA;
        var z1 = -p.X * _sinA + p.Z * _cosA;
        var y1 = p.Y;
        var y2 = y1 * _cosE - z1 * _sinE;
        var z2 = y1 * _sinE + z1 * _cosE;
        var denom = z2 + _camDistance;
        return (new SKPoint(_cx + _scale * x1 / denom, _cy - _scale * y2 / denom), z2);
    }

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        if (!IsSupportedOnCurrentPlatform()) return;

        _cx = (float)area.Width * 0.58f;  // bias right of centre so the tower clears the floating column
        _cy = (float)area.Height / 2f;
        _scale = (float)Math.Min(area.Width, area.Height) * 1.2f;
        _cosE = MathF.Cos(_elevation);
        _sinE = MathF.Sin(_elevation);
        _cosA = MathF.Cos(_azimuth);
        _sinA = MathF.Sin(_azimuth);

        // Backdrop first: ground glow, street grid, city blocks (orbit with the scene).
        DrawGround(canvas);
        DrawRoads(canvas);
        DrawCity(canvas);

        var activeStorey = OfficeTopStorey - Math.Clamp(ActiveFloorIndex, 0, DataFloorCount - 1);
        var flaggedStorey = OfficeTopStorey - FlaggedDataIndex;
        var centerDepth = Project(new Vec3(0, 0, 0)).depth; // tower centre, for backface culling

        _faces.Clear();
        foreach (var storey in _storeys)
        {
            for (var i = 0; i < 8; i++) (_proj[i], _projDepth[i]) = Project(storey.Corners[i]);
            var s = storey.Floor - 1;
            if (s == activeStorey)
            {
                float ay = 0; for (var i = 0; i < 8; i++) ay += _proj[i].Y;
                _anchorY = ay / 8f; // screen-space centre of the lit floor
            }
            for (var k = 0; k < FloorBox.Faces.Length; k++)
            {
                var (a, b, c, d) = FloorBox.Faces[k];
                var avg = (_projDepth[a] + _projDepth[b] + _projDepth[c] + _projDepth[d]) / 4f;
                if (k != 4 && avg > centerDepth) continue; // cull back-facing side faces
                _faces.Add(new Face(s, k, _proj[a], _proj[b], _proj[c], _proj[d], avg, 0));
            }
        }
        ActiveAnchorY = _anchorY;

        // Second tower faces, into the same depth-sorted pass so the two buildings occlude correctly.
        var t2Center = Project(new Vec3(Tower2OffX, _groundY + Tower2Floors * FloorHeightModel / 2f, Tower2OffZ)).depth;
        foreach (var storey in _tower2)
        {
            for (var i = 0; i < 8; i++) (_proj[i], _projDepth[i]) = Project(storey.Corners[i]);
            var s = storey.Floor - 1;
            for (var k = 0; k < FloorBox.Faces.Length; k++)
            {
                var (a, b, c, d) = FloorBox.Faces[k];
                var avg = (_projDepth[a] + _projDepth[b] + _projDepth[c] + _projDepth[d]) / 4f;
                if (k != 4 && avg > t2Center) continue;
                _faces.Add(new Face(s, k, _proj[a], _proj[b], _proj[c], _proj[d], avg, 1));
            }
        }

        _faces.Sort(static (l, r) => r.Depth.CompareTo(l.Depth));

        var pulse = 0.5f + 0.5f * MathF.Sin(_t * 2.2f);

        foreach (var f in _faces)
        {
            var isMain = f.Building == 0;
            var isActive = isMain && f.Storey == activeStorey;
            var isFlagged = isMain && f.Storey == flaggedStorey;
            var isTop = f.Kind == 4;
            var glass = isMain ? BodyBlue : Tower2Glass;

            SetQuad(_facePath, f.P0, f.P1, f.P2, f.P3);

            if (isFlagged)
                _fill.Color = GlowGreen.WithAlpha((byte)(0x55 + 0x30 * pulse)); // green optimization band
            else if (isActive)
                _fill.Color = LiveCyan.WithAlpha((byte)(0x34 + 0x24 * pulse));
            else
                _fill.Color = glass.WithAlpha(isTop ? (byte)0x40 : (byte)0x58);
            canvas.DrawPath(_facePath, _fill);

            if (!isTop)
            {
                DrawWindows(canvas, f, isActive, isFlagged, pulse);
                DrawMullions(canvas, f);
            }

            if (isFlagged)
            {
                _glow.Color = GlowGreen.WithAlpha((byte)(0x66 + 0x40 * pulse));
                canvas.DrawPath(_facePath, _glow);
                _stroke.Color = GlowGreen;
                _stroke.StrokeWidth = 1.6f;
            }
            else if (isActive)
            {
                _glow.Color = LiveCyan.WithAlpha((byte)(0x2A + 0x26 * pulse));
                canvas.DrawPath(_facePath, _glow);
                _stroke.Color = LiveCyan.WithAlpha(0xDD);
                _stroke.StrokeWidth = 1.2f;
            }
            else
            {
                _stroke.Color = glass.WithAlpha(isTop ? (byte)0x70 : (byte)0xB0);
                _stroke.StrokeWidth = 1.0f;
            }
            canvas.DrawPath(_facePath, _stroke);
        }

        // Elevators ride the core in front of the glass, in amber (distinct from the floor green).
        DrawElevators(canvas);

        // Energy link: glowing arc + travelling particles between the two towers.
        DrawConnection(canvas);
    }

    // ── Cityscape backdrop ──────────────────────────────────────────────────────────────────────
    private void DrawGround(SKCanvas canvas)
    {
        const float r = 6.0f;
        var bl = Project(new Vec3(-r, _groundY, -r)).pt;
        var br = Project(new Vec3(r, _groundY, -r)).pt;
        var fr = Project(new Vec3(r, _groundY, r)).pt;
        var fl = Project(new Vec3(-r, _groundY, r)).pt;
        var center = Project(new Vec3(0, _groundY, 0)).pt;
        var radius = Math.Max(Dist(center, bl), Dist(center, fr));

        SetQuad(_facePath, bl, br, fr, fl);
        using var shader = SKShader.CreateRadialGradient(
            center, radius,
            new[] { new SKColor(0x1B, 0x29, 0x46), new SKColor(0x0B, 0x10, 0x1C), new SKColor(0x08, 0x0B, 0x12) },
            new[] { 0f, 0.6f, 1f }, SKShaderTileMode.Clamp);
        _groundPaint.Shader = shader;
        canvas.DrawPath(_facePath, _groundPaint);
        _groundPaint.Shader = null;
    }

    private void DrawCity(SKCanvas canvas)
    {
        _cityFaces.Clear();
        foreach (var box in _city)
        {
            for (var i = 0; i < 8; i++) (_proj[i], _projDepth[i]) = Project(box.Corners[i]);
            var centerDepth = 0f;
            for (var i = 0; i < 8; i++) centerDepth += _projDepth[i];
            centerDepth /= 8f;
            for (var k = 0; k < FloorBox.Faces.Length; k++)
            {
                var (ia, ib, ic, id) = FloorBox.Faces[k];
                var avg = (_projDepth[ia] + _projDepth[ib] + _projDepth[ic] + _projDepth[id]) / 4f;
                var top = k == 4;
                if (!top && avg > centerDepth) continue; // cull back-facing walls
                _cityFaces.Add(new CityFace(_proj[ia], _proj[ib], _proj[ic], _proj[id], avg, top, box.Tone));
            }
        }
        _cityFaces.Sort(static (l, r) => r.Depth.CompareTo(l.Depth));

        foreach (var f in _cityFaces)
        {
            SetQuad(_facePath, f.A, f.B, f.C, f.D);
            // Neumorphic flat shade: lit rooftops, walls fall into soft shadow (no per-face shader).
            _cityFill.Color = f.Top
                ? Lerp(CityDark, CityLight, Math.Clamp(f.Tone, 0f, 1f))
                : Lerp(CityDark, CityLight, Math.Clamp(0.40f * f.Tone, 0f, 1f));
            canvas.DrawPath(_facePath, _cityFill);
            if (f.Top) canvas.DrawPath(_facePath, _cityEdge); // top-edge catch-light
        }
    }

    private static float Dist(SKPoint a, SKPoint b) => MathF.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

    private void DrawRoads(SKCanvas canvas)
    {
        foreach (var road in _roads)
            canvas.DrawLine(Project(road[0]).pt, Project(road[1]).pt, _roadPaint);
    }

    // ── Energy link between the two towers ───────────────────────────────────────────────────────
    private void DrawConnection(SKCanvas canvas)
    {
        // Start on the main tower's flagged floor, offset toward tower 2; end at tower 2's roof.
        var flaggedStorey = OfficeTopStorey - FlaggedDataIndex;
        var fc = _storeys[flaggedStorey].Corners;
        var flaggedY = (fc[0].Y + fc[4].Y) / 2f;
        var dirLen = MathF.Sqrt(Tower2OffX * Tower2OffX + Tower2OffZ * Tower2OffZ);
        var dx = Tower2OffX / dirLen;
        var dz = Tower2OffZ / dirLen;

        var start = Project(new Vec3(dx * 0.95f, flaggedY, dz * 0.95f)).pt;
        var end = Project(new Vec3(Tower2OffX, _groundY + Tower2Floors * FloorHeightModel, Tower2OffZ)).pt;

        // Two separate arcs, one per direction: an upper arc carries cyan main → tower 2, a lower
        // arc carries mint tower 2 → main, so each flow direction has its own clear path.
        var mx = (start.X + end.X) / 2f;
        var my = (start.Y + end.Y) / 2f;
        var lift = Dist(start, end) * 0.30f;
        var upper = new SKPoint(mx, my - lift);
        var lower = new SKPoint(mx, my + lift * 0.55f);

        DrawArc(canvas, start, upper, end, EnergyCyan, forward: true);
        DrawArc(canvas, start, lower, end, EnergyMint, forward: false);
    }

    private void DrawArc(SKCanvas canvas, SKPoint a, SKPoint c, SKPoint b, SKColor color, bool forward)
    {
        _facePath.Reset();
        _facePath.MoveTo(a);
        _facePath.QuadTo(c.X, c.Y, b.X, b.Y);
        _arcGlow.Color = color.WithAlpha(0x3A);
        _arc.Color = color.WithAlpha(0xA0);
        canvas.DrawPath(_facePath, _arcGlow);
        canvas.DrawPath(_facePath, _arc);
        DrawParticleStream(canvas, a, c, b, forward, color, 6);
    }

    private void DrawParticleStream(SKCanvas canvas, SKPoint a, SKPoint c, SKPoint b, bool forward, SKColor color, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var t = _t * 0.22f + i / (float)count;
            t -= MathF.Floor(t);
            if (!forward) t = 1f - t;            // travel the opposite way along the same arc
            var p = Bezier(a, c, b, t);
            var fade = MathF.Sin(t * MathF.PI);  // dim at the ends, bright mid-flight
            _particleGlow.Color = color.WithAlpha((byte)(0x90 * fade));
            _particle.Color = color.WithAlpha((byte)(0xC0 + 0x3F * fade));
            canvas.DrawCircle(p, 5.5f, _particleGlow);
            canvas.DrawCircle(p, 2.4f, _particle);
        }
    }

    private static SKPoint Bezier(SKPoint a, SKPoint c, SKPoint b, float t)
    {
        var mt = 1f - t;
        var w0 = mt * mt;
        var w1 = 2f * mt * t;
        var w2 = t * t;
        return new SKPoint(w0 * a.X + w1 * c.X + w2 * b.X, w0 * a.Y + w1 * c.Y + w2 * b.Y);
    }

    // ── Elevators ───────────────────────────────────────────────────────────────────────────────
    private void DrawElevators(SKCanvas canvas)
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
        for (var s = 0; s < 4; s++)
        {
            var (b0, b1, t1, t0) = sides[s];
            var (q0, d0) = Project(bottom[b0]);
            var (q1, d1) = Project(bottom[b1]);
            var (q2, d2) = Project(top[t1]);
            var (q3, d3) = Project(top[t0]);
            var avg = (d0 + d1 + d2 + d3) / 4f;
            if (avg < bestDepth) { bestDepth = avg; bestSide = s; _quad[0] = q0; _quad[1] = q1; _quad[2] = q2; _quad[3] = q3; }
        }
        if (bestSide < 0) return;

        SKPoint Q(float u, float v) => Lerp(Lerp(_quad[0], _quad[1], u), Lerp(_quad[3], _quad[2], u), v);

        const float vTop = 0.06f, vBot = 0.94f, carH = 0.05f, halfW = 0.022f;
        for (var e = 0; e < ElevatorShafts; e++)
        {
            var u = 0.30f + e * (0.40f / Math.Max(1, ElevatorShafts - 1)); // cluster the shafts near the core

            // Shaft channel: a translucent fill of the elevator colour itself.
            SetQuad(_facePath, Q(u - halfW, vTop), Q(u + halfW, vTop), Q(u + halfW, vBot), Q(u - halfW, vBot));
            canvas.DrawPath(_facePath, _shaftPaint);

            canvas.DrawLine(Q(u - halfW, vTop), Q(u - halfW, vBot), _railPaint);
            canvas.DrawLine(Q(u + halfW, vTop), Q(u + halfW, vBot), _railPaint);

            // Car oscillates up/down; alternate directions and speeds per shaft.
            var dir = e % 2 == 0 ? 1f : -1f;
            var speed = 0.6f + e * 0.25f;
            var vc = vTop + (vBot - vTop) * (0.5f + 0.5f * MathF.Sin(_t * speed * dir + e * 1.7f));

            // Lead car drives the synced left-column metric (floor number ticks as it moves).
            if (e == 0)
            {
                var frac = (vc - vTop) / (vBot - vTop);
                var floor = Math.Clamp(1 + (int)MathF.Round(frac * (TotalStoreys - 1)), 1, TotalStoreys);
                if (floor != _lastElevFloor)
                {
                    ElevatorDirText = floor > _lastElevFloor ? "▲ Ascending" : "▼ Descending";
                    _lastElevFloor = floor;
                    ElevatorFloorText = floor + "F";
                }
            }

            SetQuad(_cellPath,
                Q(u - halfW, vc - carH * 0.5f), Q(u + halfW, vc - carH * 0.5f),
                Q(u + halfW, vc + carH * 0.5f), Q(u - halfW, vc + carH * 0.5f));
            canvas.DrawPath(_cellPath, _carGlow);
            canvas.DrawPath(_cellPath, _carPaint);
        }
    }

    private void DrawWindows(SKCanvas canvas, Face f, bool isActive, bool isFlagged, float pulse)
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

                _fill.Color = baseColor.WithAlpha((byte)Math.Clamp(level * 255f, 0, 255));
                SetQuad(_cellPath, p.A, p.B, p.C, p.D);
                canvas.DrawPath(_cellPath, _fill);
            }
        }
    }

    private void DrawMullions(SKCanvas canvas, Face f)
    {
        for (var m = 1; m < WindowsPerFace; m++)
        {
            var u = m / (float)WindowsPerFace;
            canvas.DrawLine(Lerp(f.P0, f.P1, u), Lerp(f.P3, f.P2, u), _mullion);
        }
    }

    private static void SetQuad(SKPath path, SKPoint a, SKPoint b, SKPoint c, SKPoint d)
    {
        path.Reset();
        path.MoveTo(a); path.LineTo(b); path.LineTo(c); path.LineTo(d); path.Close();
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
