using SketchPen.Plot.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace SketchPen.Plot.Debug;

internal class DebugPlotContext : IPlotContext
{
    private readonly IDictionary<string, object> _globals;

    public DebugPlotContext()
    {
        _globals = new Dictionary<string, object>();
    }

    public ICanvas Canvas => throw new NotImplementedException();

    public PlotColor PenColor { get;  set; }
    public float PenWidth { get;  set; }
    public PenCap PenCap { get;  set; }
    public float MaxPenWidth { get; set; }
    public float MinPenWidth { get; set; }
    public PlotColor BrushColor { get; set; }
    public PlotColor GradientBrushColor { get; set; }
    public CanvasPoint GradientBrushPoint1 { get; set; }
    public CanvasPoint GradientBrushPoint2 { get; set; }

    public IDictionary<string, object> Globals => _globals;

    public IBrush CreateBrush(IEnumerable<object> parameters)
    {
        throw new NotImplementedException();
    }

    public IPen CreatePen(IEnumerable<object> parameters)
    {
        throw new NotImplementedException();
    }

    public IPlotPath CreatePlotPath()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        
    }

    public byte[] Encode(EncodeFormat format)
    {
        return Array.Empty<byte>();
    }

    public void Init(int width, int height)
    {
        
    }

    public void Init(int width, int height, object canvasObject)
    {
        
    }

    public CanvasPoint Project(CanvasPoint point)
    {
        return point;
    }

    public CanvasRectangle Project(CanvasRectangle rect)
    {
        return rect;
    }

    public float Project(float number)
    {
        return number;
    }

    public void ResetTransform()
    {
        
    }
}
