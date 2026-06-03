using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using SmartCity.Models;

namespace SmartCity.Presentation.Controls;

/// <summary>
/// Energy-consumption bar chart drawn with SkiaSharp (custom, Tier 4: bespoke neon look, no Toolkit
/// equivalent; reuses the SkiaSharp dependency already pinned for the twin). Each slot shows the
/// current consumption as a dim body-blue bar with the displayed value overlaid; on
/// <see cref="Applied"/> the overlay animates down from current to optimized, revealing the saving.
/// </summary>
public sealed partial class BarChartView : SKCanvasElement
{
    private static readonly SKColor BodyBlueDim = new(0x2A, 0x3A, 0x6E);
    private static readonly SKColor BodyBlue = new(0x5B, 0x82, 0xFF);
    private static readonly SKColor Savings = new(0x46, 0xE0, 0x8C);
    private static readonly SKColor Muted = new(0x8A, 0x94, 0xA6);

    private float _progress;          // 0 = current heights, 1 = optimized heights
    private readonly DispatcherTimer _anim;

    public static readonly DependencyProperty SeriesProperty =
        DependencyProperty.Register(nameof(Series), typeof(IEnumerable<HourlyConsumption>), typeof(BarChartView),
            new PropertyMetadata(null, static (d, _) => ((BarChartView)d).Invalidate()));

    public static readonly DependencyProperty AppliedProperty =
        DependencyProperty.Register(nameof(Applied), typeof(bool), typeof(BarChartView),
            new PropertyMetadata(false, static (d, _) => ((BarChartView)d).OnAppliedChanged()));

    public IEnumerable<HourlyConsumption>? Series
    {
        get => (IEnumerable<HourlyConsumption>?)GetValue(SeriesProperty);
        set => SetValue(SeriesProperty, value);
    }

    public bool Applied
    {
        get => (bool)GetValue(AppliedProperty);
        set => SetValue(AppliedProperty, value);
    }

    public BarChartView()
    {
        _anim = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _anim.Tick += OnAnimTick;
        Unloaded += (_, _) => { _anim.Stop(); _anim.Tick -= OnAnimTick; };
    }

    private void OnAppliedChanged() => _anim.Start(); // ease toward the new target each tick

    private void OnAnimTick(object? sender, object e)
    {
        var target = Applied ? 1f : 0f;
        _progress += (target - _progress) * 0.18f;     // ease-out toward target (~400ms)
        if (MathF.Abs(target - _progress) < 0.004f) { _progress = target; _anim.Stop(); }
        Invalidate();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var w = double.IsInfinity(availableSize.Width) ? 320 : availableSize.Width;
        var h = double.IsInfinity(availableSize.Height) ? 160 : availableSize.Height;
        return new Size(w, h);
    }

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        if (!IsSupportedOnCurrentPlatform()) return;
        var data = Series?.ToList();
        if (data is null || data.Count == 0) return;

        var w = (float)area.Width;
        var h = (float)area.Height;
        const float padBottom = 22f, padTop = 12f;
        var plotH = h - padBottom - padTop;
        var max = data.Max(d => d.CurrentKwh);
        if (max <= 0) return;

        var slot = w / data.Count;
        var barW = slot * 0.46f;

        using var dim = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = BodyBlueDim };
        using var bar = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var label = new SKPaint { IsAntialias = true, Color = Muted };
        using var font = new SKFont(SKTypeface.Default, 10.5f);

        for (var i = 0; i < data.Count; i++)
        {
            var d = data[i];
            var cx = slot * i + slot / 2f;
            var displayed = d.CurrentKwh + (d.OptimizedKwh - d.CurrentKwh) * _progress;

            var curH = (float)(d.CurrentKwh / max) * plotH;
            var dispH = (float)(displayed / max) * plotH;
            var baseY = padTop + plotH;

            // Dim current bar (the "before") sits behind the displayed bar so the gap reads as savings.
            var rectCur = new SKRect(cx - barW / 2f, baseY - curH, cx + barW / 2f, baseY);
            canvas.DrawRoundRect(rectCur, 3f, 3f, dim);

            // Displayed bar: blue while at current, shifting toward green as it drops to optimized.
            bar.Color = Lerp(BodyBlue, Savings, _progress).WithAlpha(0xF0);
            var rectDisp = new SKRect(cx - barW / 2f, baseY - dispH, cx + barW / 2f, baseY);
            canvas.DrawRoundRect(rectDisp, 3f, 3f, bar);

            var isNow = string.Equals(d.TimeLabel, "Now", StringComparison.OrdinalIgnoreCase);
            label.Color = isNow ? Savings : Muted;
            canvas.DrawText(d.TimeLabel, cx, h - 6f, SKTextAlign.Center, font, label);
        }
    }

    private static SKColor Lerp(SKColor a, SKColor b, float t) => new(
        (byte)(a.Red + (b.Red - a.Red) * t),
        (byte)(a.Green + (b.Green - a.Green) * t),
        (byte)(a.Blue + (b.Blue - a.Blue) * t));
}
