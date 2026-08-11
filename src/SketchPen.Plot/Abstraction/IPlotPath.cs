using System;
using System.Collections.Generic;

namespace SketchPen.Plot.Abstraction;

public interface IPlotPath : IDisposable
{
    object EngineElement { get; }

    void Start();
    void Close();

    void AddLines(IEnumerable<CanvasPoint> points);
    void AddArc(CanvasRectangle rect, float startAngle, float sweepAngle);
    void AddPoint(CanvasPoint point);
    void AddCubic(CanvasPoint controlPoint1, CanvasPoint controlPoint2, CanvasPoint end);
    void AddQuad(CanvasPoint controlPoint, CanvasPoint end);
}
