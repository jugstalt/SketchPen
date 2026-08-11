using SketchPen.Plot.Abstraction;

namespace SketchPen.Plot.Debugging;

internal class DebugCanvas : ICanvas
{
    private ICanvas? _canvas;

    public DebugCanvas() { }

    public DebugCanvas(ICanvas? canvas)
    {
        _canvas = canvas;
    }

    public void Dispose()
    {
        _canvas?.Dispose();
    }

    public void DrawArc(IPen pen, CanvasRectangle rect, float startAngle, float sweepAngle)
    {
        _canvas?.DrawArc(pen, rect, startAngle, sweepAngle);
    }

    public void DrawEllipse(IPen pen, CanvasRectangle rect)
    {
        _canvas?.DrawEllipse(pen, rect);
    }

    public void DrawLine(IPen pen, CanvasPoint p1, CanvasPoint p2)
    {
        _canvas?.DrawLine(pen, p1, p2);
    }

    public void DrawPath(IPen pen, IPlotPath path)
    {
        _canvas?.DrawPath(pen, path);
    }

    public void FillEllipse(IBrush brush, CanvasRectangle rect)
    {
        _canvas?.FillEllipse(brush, rect);
    }

    public void DrawRect(IPen pen, CanvasRectangle rect, float cornerRadius = 0)
    {
        _canvas?.DrawRect(pen, rect, cornerRadius);
    }

    public void FillRect(IBrush brush, CanvasRectangle rect, float cornerRadius = 0)
    {
        _canvas?.FillRect(brush, rect, cornerRadius);
    }

    public void FillPath(IBrush brush, IPlotPath path)
    {
        _canvas?.FillPath(brush, path);
    }

    public void FillPie(IBrush brush, CanvasRectangle rect, float startAngle, float sweepAngle)
    {
        _canvas?.FillPie(brush, rect, startAngle, sweepAngle);
    }

    public void RotateTransform(float angle)
    {
        _canvas?.RotateTransform(angle);
    }

    public void RotateTransform(float angle, float pivotX, float pivotY)
    {
        _canvas?.RotateTransform(angle, pivotX, pivotY);
    }

    public void ScaleTransform(float sx, float sy)
    {
        _canvas?.ScaleTransform(sx, sy);
    }

    public void TranslateTransform(float dx, float dy)
    {
        _canvas?.TranslateTransform(dx, dy);
    }

    public void DrawImage(IPlotContext sourceContext, CanvasRectangle dest, CanvasRectangle source)
    {
        _canvas?.DrawImage(sourceContext, dest, source);
    }

    public void DrawText(IFont font, string text, CanvasPoint position, IBrush? brush = null)
    {
        _canvas?.DrawText(font, text, position);
    }
}
