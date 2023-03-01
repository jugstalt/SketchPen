using SketchPen.Plot.Abstraction;
using System.Collections.Generic;

namespace SketchPen.Plot.Debugging;

internal class DebugPlotPath : IPlotPath
{
    private object _engineElement = new object();

    public object EngineElement => _engineElement;

    public void AddArc(CanvasRectangle rect, float startAngle, float sweepAngle)
    {

    }

    public void AddLines(IEnumerable<CanvasPoint> points)
    {

    }

    public void AddPoint(CanvasPoint point)
    {

    }

    public void Close()
    {

    }

    public void Dispose()
    {

    }

    public void Start()
    {

    }
}
