using System;
using System.Collections.Generic;
using System.Drawing;

namespace SketchPen.Plot.Abstraction
{
    public interface IPlotContext : IDisposable
    {
        Graphics GraphicsContext { get; }

        Color PenColor { set; }
        float PenWidth { set; }
        PenCap PenCap { set; }

        float MaxPenWidth { set; }
        float MinPenWidth { set; }

        Color BrushColor { set; }

        Color GradientBrushColor { set; }
        PointF GradientBrushPoint1 { set; }
        PointF GradientBrushPoint2 { set; }

        IDictionary<string, object> Globals { get; }

        IPen CreatePen(IEnumerable<object> parameters = null);
        IBrush CreateBrush(IEnumerable<object> parameters = null);

        PointF Project(PointF point);
        RectangleF Project(RectangleF rect);
        float Project(float number);

        void ResetTransform();
    }
}
