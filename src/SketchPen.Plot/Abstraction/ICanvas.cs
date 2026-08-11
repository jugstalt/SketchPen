using System;

namespace SketchPen.Plot.Abstraction;

public interface ICanvas : IDisposable
{
    void TranslateTransform(float dx, float dy);
    void RotateTransform(float angle);
    void RotateTransform(float angle, float pivotX, float pivotY);
    void ScaleTransform(float sx, float sy);

    void DrawLine(IPen pen, CanvasPoint p1, CanvasPoint p2);

    void DrawEllipse(IPen pen, CanvasRectangle rect);
    void FillEllipse(IBrush brush, CanvasRectangle rect);

    void DrawRect(IPen pen, CanvasRectangle rect, float cornerRadius = 0);
    void FillRect(IBrush brush, CanvasRectangle rect, float cornerRadius = 0);

    void DrawArc(IPen pen, CanvasRectangle rect, float startAngle, float sweepAngle);
    void FillPie(IBrush brush, CanvasRectangle rect, float startAngle, float sweepAngle);

    void DrawPath(IPen pen, IPlotPath path);
    void FillPath(IBrush brush, IPlotPath path);

    void DrawImage(IPlotContext sourceContext, CanvasRectangle dest, CanvasRectangle source);

    void DrawText(IFont font, string text, CanvasPoint position, IBrush? brush = null);
}
