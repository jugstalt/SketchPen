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
}
