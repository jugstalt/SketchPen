using SkiaSharp;

namespace SketchPen.Plot.Skia.Extensions;

static class CanvasObjectExtensions
{
    static public SKPoint ToSKPoint(this CanvasPoint point)
    {
        return new SKPoint(point.X, point.Y);
    }


    static public SKRect ToSKRect(this CanvasRectangle rect)
    {
        return new SKRect(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
    }

    static public SKColor ToSKColor(this PlotColor color)
    {
        return new SKColor(color.R, color.G, color.B, color.A);
    }
}
