using System.Diagnostics;
using Microsoft.UI.Xaml.Input;
using OrbFidget.Helpers;
using OrbFidget.Models;
using OrbFidget.Services;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

namespace OrbFidget.Controls;

public class OrbCanvas : SKCanvasElement
{
    private readonly OrbState _state = new();
    private readonly OrbPhysicsEngine _physics = new();
    private readonly IOrbHapticService _haptics;
    private readonly IOrbAudioEngine _audio;

    private readonly DispatcherTimer _timer;
    private readonly Stopwatch _stopwatch = new();
    private long _lastFrameTicks;

    // Reusable paint objects for performance
    private readonly SKPaint _bgPaint = new() { IsAntialias = true };
    private readonly SKPaint _dotPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _glowPaint = new() { IsAntialias = true };
    private readonly SKPaint _linePaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };
    private readonly SKPaint _trailPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _particlePaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _orbPaint = new() { IsAntialias = true };
    private readonly SKPaint _rimPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
    private readonly SKPaint _textPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _milestonePaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };
    private readonly SKFont _comboFont = new(SKTypeface.Default, 11f);

    // Cached dot grid path
    private SKPath? _dotGridPath;
    private float _cachedGridW, _cachedGridH;

    private uint? _capturedPointerId;

    public OrbCanvas(IOrbHapticService haptics, IOrbAudioEngine audio)
    {
        _haptics = haptics;
        _audio = audio;

        ManipulationMode = ManipulationModes.All;

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerCanceled += OnPointerCanceled;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += OnGameTick;

        _stopwatch.Start();
        _lastFrameTicks = _stopwatch.ElapsedTicks;
        _timer.Start();
    }

    private void OnGameTick(object? sender, object e)
    {
        long now = _stopwatch.ElapsedTicks;
        float dt = (float)(now - _lastFrameTicks) / Stopwatch.Frequency;
        _lastFrameTicks = now;
        dt = MathF.Min(dt, OrbPhysicsEngine.DtClamp);

        // Stretch tone throttle during drag
        if (_state.IsDragging)
        {
            float dist = _physics.GetStretchDistance(_state);
            _state.TickAccumulator += dist * dt;
            if (_state.TickAccumulator > OrbPhysicsEngine.StretchToneThreshold)
            {
                _audio.PlayStretchTone(dist);
                _state.TickAccumulator = 0;
            }
        }

        _physics.Update(_state, dt);

        // Check milestones during drag
        int milestone = _physics.CheckMilestones(_state);
        if (milestone >= 0)
        {
            _haptics.OnMilestone(milestone);
            _audio.PlayMilestoneClick();
        }

        Invalidate();
    }

    // --- Input Handlers ---

    private (float x, float y) ToCenter(PointerRoutedEventArgs e, Size area)
    {
        var pos = e.GetCurrentPoint(this).Position;
        float cx = (float)ActualWidth / 2f;
        float cy = (float)ActualHeight / 2f;
        return ((float)pos.X - cx, (float)pos.Y - cy);
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var (px, py) = ToCenter(e, default);
        if (_physics.TryGrab(_state, px, py))
        {
            _capturedPointerId = e.Pointer.PointerId;
            CapturePointer(e.Pointer);
            _haptics.OnGrab();
            e.Handled = true;
        }
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_state.IsDragging) return;
        if (_capturedPointerId != null && e.Pointer.PointerId != _capturedPointerId) return;

        var (px, py) = ToCenter(e, default);
        _physics.UpdatePointer(_state, px, py);
        e.Handled = true;
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        DoRelease(e);
    }

    private void OnPointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        DoRelease(e);
    }

    private void DoRelease(PointerRoutedEventArgs e)
    {
        if (!_state.IsDragging) return;

        var (intensity, _, isCombo) = _physics.Release(_state);

        if (intensity > OrbPhysicsEngine.SnapMinIntensity)
        {
            _haptics.OnRelease(intensity);
            _audio.PlaySnapChime(intensity);
        }

        if (_capturedPointerId != null)
        {
            ReleasePointerCapture(e.Pointer);
            _capturedPointerId = null;
        }

        e.Handled = true;
    }

    // --- Rendering ---

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        float w = (float)area.Width;
        float h = (float)area.Height;
        float cx = w / 2f;
        float cy = h / 2f;

        canvas.Clear(ColorHelper.BackgroundDark);

        // 1. Background radial gradient
        DrawBackground(canvas, w, h, cx, cy);

        // 2. Dot grid
        DrawDotGrid(canvas, w, h);

        float dist = _physics.GetStretchDistance(_state);
        float screenOrbX = cx + _state.OrbX;
        float screenOrbY = cy + _state.OrbY;
        float finalRadius = OrbPhysicsEngine.OrbRadius * _state.CurrentScale;

        // 3. Anchor glow + crosshair
        if (dist > 10f)
            DrawAnchor(canvas, cx, cy, dist);

        // 4. Stretch line
        if (dist > 8f)
            DrawStretchLine(canvas, cx, cy, screenOrbX, screenOrbY, dist);

        // 5. Trail dots
        DrawTrail(canvas, cx, cy);

        // 6. Particles
        DrawParticles(canvas, cx, cy);

        // 7. Milestone rings (drag only)
        if (_state.IsDragging)
            DrawMilestoneRings(canvas, cx, cy);

        // 8. Orb outer glow
        DrawOrbGlow(canvas, screenOrbX, screenOrbY, finalRadius, dist);

        // 9. Orb body
        DrawOrbBody(canvas, screenOrbX, screenOrbY, finalRadius, dist);

        // 10. Specular highlight
        DrawSpecular(canvas, screenOrbX, screenOrbY, finalRadius);

        // 11. Rim light
        DrawRimLight(canvas, screenOrbX, screenOrbY, finalRadius);

        // 12. Combo text
        DrawComboText(canvas, screenOrbX, screenOrbY, finalRadius);
    }

    private void DrawBackground(SKCanvas canvas, float w, float h, float cx, float cy)
    {
        float radius = MathF.Max(w, h) * 0.6f;
        var shader = SKShader.CreateRadialGradient(
            new SKPoint(cx, cy), radius,
            new[] { ColorHelper.BackgroundMid, ColorHelper.BackgroundDark },
            new[] { 0f, 1f },
            SKShaderTileMode.Clamp);

        _bgPaint.Shader = shader;
        canvas.DrawRect(0, 0, w, h, _bgPaint);
        _bgPaint.Shader = null;
        shader.Dispose();
    }

    private void DrawDotGrid(SKCanvas canvas, float w, float h)
    {
        // Rebuild cached path if size changed
        if (_dotGridPath == null ||
            MathF.Abs(_cachedGridW - w) > 1f ||
            MathF.Abs(_cachedGridH - h) > 1f)
        {
            _dotGridPath?.Dispose();
            _dotGridPath = new SKPath();
            _cachedGridW = w;
            _cachedGridH = h;

            for (float x = 0; x < w; x += OrbPhysicsEngine.DotGridSpacing)
            {
                for (float y = 0; y < h; y += OrbPhysicsEngine.DotGridSpacing)
                {
                    _dotGridPath.AddCircle(x, y, OrbPhysicsEngine.DotGridRadius);
                }
            }
        }

        _dotPaint.Color = new SKColor(100, 90, 80, (byte)(0.04f * 255));
        canvas.DrawPath(_dotGridPath, _dotPaint);
    }

    private void DrawAnchor(SKCanvas canvas, float cx, float cy, float dist)
    {
        // Anchor glow
        float glowAlpha = MathF.Min(dist / 200f, 0.3f);
        var glowShader = SKShader.CreateRadialGradient(
            new SKPoint(cx, cy), 30f,
            new[] { ColorHelper.AccentColor.WithAlphaF(glowAlpha), SKColors.Transparent },
            new[] { 0f, 1f },
            SKShaderTileMode.Clamp);

        _glowPaint.Shader = glowShader;
        canvas.DrawCircle(cx, cy, 30f, _glowPaint);
        _glowPaint.Shader = null;
        glowShader.Dispose();

        // Crosshair
        float crossAlpha = MathF.Min(dist / 300f, 0.15f);
        _linePaint.Color = ColorHelper.AccentColor.WithAlphaF(crossAlpha);
        _linePaint.StrokeWidth = 0.5f;
        _linePaint.PathEffect = null;
        canvas.DrawLine(cx - 12, cy, cx + 12, cy, _linePaint);
        canvas.DrawLine(cx, cy - 12, cx, cy + 12, _linePaint);
    }

    private void DrawStretchLine(SKCanvas canvas, float cx, float cy, float orbX, float orbY, float dist)
    {
        float alpha = MathF.Min(dist / 200f, 0.5f) * 0.3f;
        _linePaint.Color = ColorHelper.AccentColor.WithAlphaF(alpha);
        _linePaint.StrokeWidth = 2f + dist * 0.01f;
        _linePaint.PathEffect = SKPathEffect.CreateDash(new[] { 4f, 6f }, 0);
        canvas.DrawLine(cx, cy, orbX, orbY, _linePaint);
        _linePaint.PathEffect = null;
    }

    private void DrawTrail(SKCanvas canvas, float cx, float cy)
    {
        int i = 0;
        int count = _state.Trail.Count;
        foreach (var dot in _state.Trail)
        {
            float ratio = (float)i / count;
            float radius = ratio * 6f;
            float alpha = ratio * 0.15f;
            _trailPaint.Color = ColorHelper.AccentColor.WithAlphaF(alpha);
            canvas.DrawCircle(cx + dot.X, cy + dot.Y, radius, _trailPaint);
            i++;
        }
    }

    private void DrawParticles(SKCanvas canvas, float cx, float cy)
    {
        foreach (var p in _state.Particles)
        {
            float lifeRatio = p.Life / p.MaxLife;
            float alpha = lifeRatio * 0.8f;
            float size = p.Size * lifeRatio;

            _particlePaint.Color = ColorHelper.FromHsla(p.Hue, 70f, 65f, alpha);
            canvas.DrawCircle(cx + p.X, cy + p.Y, size, _particlePaint);
        }
    }

    private void DrawMilestoneRings(SKCanvas canvas, float cx, float cy)
    {
        int[] thresholds = OrbPhysicsEngine.MilestoneThresholds;
        float[] unhitAlphas = { 0.08f, 0.06f, 0.04f };
        float[] hitAlphas = { 0.25f, 0.19f, 0.13f };

        for (int i = 0; i < thresholds.Length; i++)
        {
            bool hit = _state.MilestonesHit.Contains(thresholds[i]);

            if (hit)
            {
                _milestonePaint.Color = ColorHelper.AccentColor.WithAlphaF(hitAlphas[i]);
                _milestonePaint.StrokeWidth = 1.5f;
                _milestonePaint.PathEffect = null;
            }
            else
            {
                _milestonePaint.Color = new SKColor(100, 90, 80, (byte)(unhitAlphas[i] * 255));
                _milestonePaint.StrokeWidth = 0.5f;
                _milestonePaint.PathEffect = SKPathEffect.CreateDash(new[] { 3f, 5f }, 0);
            }

            canvas.DrawCircle(cx, cy, thresholds[i], _milestonePaint);
        }

        _milestonePaint.PathEffect = null;
    }

    private void DrawOrbGlow(SKCanvas canvas, float orbX, float orbY, float finalRadius, float dist)
    {
        bool dragging = _state.IsDragging;
        float glowRadius = finalRadius + 20f + (dragging ? dist * 0.15f : 0f);
        float innerAlpha = dragging ? 0.15f : 0.08f;
        float midAlpha = dragging ? 0.06f : 0.03f;

        var shader = SKShader.CreateRadialGradient(
            new SKPoint(orbX, orbY), glowRadius,
            new[]
            {
                ColorHelper.AccentColor.WithAlphaF(innerAlpha),
                ColorHelper.AccentColor.WithAlphaF(midAlpha),
                SKColors.Transparent
            },
            new[] { 0f, 0.6f, 1f },
            SKShaderTileMode.Clamp);

        _glowPaint.Shader = shader;
        canvas.DrawCircle(orbX, orbY, glowRadius, _glowPaint);
        _glowPaint.Shader = null;
        shader.Dispose();
    }

    private void DrawOrbBody(SKCanvas canvas, float orbX, float orbY, float finalRadius, float dist)
    {
        float hueShift = dist * 0.15f;

        var color0 = ColorHelper.FromHsla(22f + hueShift, 55f, 52f, 1f);
        var color1 = ColorHelper.FromHsla(18f + hueShift, 50f, 38f, 1f);
        var color2 = ColorHelper.FromHsla(15f + hueShift, 45f, 22f, 1f);

        // Light origin offset upper-left: (-25%, -30%) relative to orb center
        float lightX = orbX - finalRadius * 0.25f;
        float lightY = orbY - finalRadius * 0.30f;

        var shader = SKShader.CreateTwoPointConicalGradient(
            new SKPoint(lightX, lightY), finalRadius * 0.1f,
            new SKPoint(orbX, orbY), finalRadius,
            new[] { color0, color1, color2 },
            new[] { 0f, 0.5f, 1f },
            SKShaderTileMode.Clamp);

        _orbPaint.Shader = shader;
        canvas.DrawCircle(orbX, orbY, finalRadius, _orbPaint);
        _orbPaint.Shader = null;
        shader.Dispose();
    }

    private void DrawSpecular(SKCanvas canvas, float orbX, float orbY, float finalRadius)
    {
        float specX = orbX - finalRadius * 0.30f;
        float specY = orbY - finalRadius * 0.35f;
        float specRadius = finalRadius * 0.6f;

        var shader = SKShader.CreateRadialGradient(
            new SKPoint(specX, specY), specRadius,
            new[]
            {
                new SKColor(255, 255, 255, (byte)(0.18f * 255)),
                new SKColor(255, 255, 255, (byte)(0.04f * 255)),
                SKColors.Transparent
            },
            new[] { 0f, 0.5f, 1f },
            SKShaderTileMode.Clamp);

        _glowPaint.Shader = shader;
        canvas.DrawCircle(specX, specY, specRadius, _glowPaint);
        _glowPaint.Shader = null;
        shader.Dispose();
    }

    private void DrawRimLight(SKCanvas canvas, float orbX, float orbY, float finalRadius)
    {
        float alpha = 0.08f + (_state.IsDragging ? 0.06f : 0f);
        _rimPaint.Color = new SKColor(255, 200, 160, (byte)(alpha * 255));
        canvas.DrawCircle(orbX, orbY, finalRadius, _rimPaint);
    }

    private void DrawComboText(SKCanvas canvas, float orbX, float orbY, float finalRadius)
    {
        if (_state.ComboCount < 2) return;

        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        float elapsed = (now - _state.LastSnapTimestamp) / 1000f;
        float alpha = MathF.Max(0, 1f - elapsed / (OrbPhysicsEngine.ComboFade / 1000f)) * 0.6f;
        if (alpha <= 0.01f) return;

        _textPaint.Color = ColorHelper.AccentColor.WithAlphaF(alpha);

        string text = $"\u00d7{_state.ComboCount}";
        canvas.DrawText(text, orbX, orbY + finalRadius + 24f, SKTextAlign.Center, _comboFont, _textPaint);
    }
}
