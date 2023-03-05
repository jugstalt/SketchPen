using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Skia.Extensions;
using SkiaSharp;
using System.Runtime.CompilerServices;

namespace SketchPen.Plot.Skia;

class Canvas : ICanvas
{
    private SKCanvas _canvas;

    public Canvas(SKBitmap bitmap)
    {
        _canvas = new SKCanvas(bitmap);
    }

    #region ICanvas

    internal SKCanvas SkCanvas => _canvas;

    public void Dispose()
    {
        if (_canvas != null)
        {
            _canvas.Dispose();
            _canvas = null;
        }
    }

    public void DrawArc(IPen pen, CanvasRectangle rect, float startAngle, float sweepAngle)
    {
        _canvas?.DrawArc(rect.ToSKRect(),
                         startAngle, sweepAngle,
                         false,
                         GetSKPaint(pen));
    }

    public void FillPie(IBrush brush, CanvasRectangle rect, float startAngle, float sweepAngle)
    {
        _canvas?.DrawArc(new SKRect(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height),
                         startAngle, sweepAngle, true,
                         GetSKPaint(brush));
    }

    public void DrawEllipse(IPen pen, CanvasRectangle rect)
    {
        _canvas?.DrawOval(new SKRect(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height), GetSKPaint(pen));
    }

    public void FillEllipse(IBrush brush, CanvasRectangle rect)
    {
        _canvas?.DrawOval(new SKRect(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height), GetSKPaint(brush));
    }

    public void DrawLine(IPen pen, CanvasPoint p1, CanvasPoint p2)
    {
        _canvas?.DrawLine(p1.ToSKPoint(), p2.ToSKPoint(), GetSKPaint(pen));
    }

    public void DrawPath(IPen pen, IPlotPath path)
    {
        _canvas?.DrawPath((SKPath)path.EngineElement, GetSKPaint(pen));
    }

    public void FillPath(IBrush brush, IPlotPath path)
    {
        _canvas?.DrawPath((SKPath)path.EngineElement, GetSKPaint(brush));
    }

    public void RotateTransform(float angle)
    {
        _canvas?.RotateDegrees(angle);
    }

    public void ScaleTransform(float sx, float sy)
    {
        _canvas?.Scale(sx, sy);
    }

    public void TranslateTransform(float dx, float dy)
    {
        _canvas?.Translate(dx, dy);
    }

    public void DrawImage(IPlotContext sourceContext, CanvasRectangle dest, CanvasRectangle source)
    {
        if (sourceContext is PlotContext skiaContext && skiaContext.Bitmap != null)
        {
            _canvas.DrawBitmap(skiaContext.Bitmap,
                               dest.ToSKRect(),
                               source.ToSKRect(),
                               new SKPaint()
                               {
                                   FilterQuality = SKFilterQuality.High
                               });

        }
    }
   
    #endregion

    #region Helper

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SKPaint GetSKPaint(IPen pen)
    {
        var skPaint = (SKPaint)pen.EngineElement;

        return skPaint;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SKPaint GetSKPaint(IBrush brush)
    {
        var skPaint = (SKPaint)brush.EngineElement;

        return skPaint;
    }

    #endregion
}
