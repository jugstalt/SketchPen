using System;
using System.Collections.Generic;
using System.Drawing;

namespace SketchPen.Plot.Abstraction
{
    public interface IPlotContext : IDisposable
    {
        IGraphicsContext GraphicsContext { get; }

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

        IPen CreatePen(IEnumerable<object> parameters = null);
        IBrush CreateBrush(IEnumerable<object> parameters = null);

        CanvasPoint Project(CanvasPoint point);
        CanvasRectangle Project(CanvasRectangle rect);

        IPlotPath CreatePlotPath();

        float Project(float number);

        void ResetTransform();
    }
}
