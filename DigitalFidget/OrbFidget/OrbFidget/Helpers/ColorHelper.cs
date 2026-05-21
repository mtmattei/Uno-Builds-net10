using SkiaSharp;

namespace OrbFidget.Helpers;

public static class ColorHelper
{
    public static readonly SKColor AccentColor = new(201, 120, 93);
    public static readonly SKColor BackgroundDark = SKColor.Parse("#141210");
    public static readonly SKColor BackgroundMid = SKColor.Parse("#1E1B17");

    public static SKColor FromHsla(float h, float s, float l, float a)
    {
        var color = SKColor.FromHsl(h, s, l);
        return color.WithAlpha((byte)(a * 255));
    }

    public static SKColor WithAlphaF(this SKColor color, float alpha)
    {
        return color.WithAlpha((byte)Math.Clamp(alpha * 255, 0, 255));
    }
}
