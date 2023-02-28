using System;
using System.Collections.Generic;

namespace SketchPen.Plot.Abstraction;

public interface IPlotContext : IDisposable
{
    void Init(int width, int height);
    void Init(int width, int height, object canvasObject);

    ICanvas Canvas { get; }

    PlotColor PenColor { set; }
    float PenWidth { set; }
    PenCap PenCap { set; }

    float MaxPenWidth { set; }
    float MinPenWidth { set; }

    PlotColor BrushColor { set; }

    PlotColor GradientBrushColor { set; }
    CanvasPoint GradientBrushPoint1 { set; }
    CanvasPoint GradientBrushPoint2 { set; }

    IDictionary<string, object> Globals { get; }

    IPen CreatePen(IEnumerable<object> parameters);
    IBrush CreateBrush(IEnumerable<object> parameters);

    CanvasPoint Project(CanvasPoint point);
    CanvasRectangle Project(CanvasRectangle rect);

    IPlotPath CreatePlotPath();

    float Project(float number);

    void ResetTransform();

    byte[] Encode(EncodeFormat format);
}
