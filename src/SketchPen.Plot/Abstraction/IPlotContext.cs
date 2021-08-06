using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SketchPen.Plot.Abstraction
{
    public interface IPlotContext : IDisposable
    {
        Graphics GraphicsContext { get; }

        Color PenColor { set; }
        float PenWidth { set; }

        Color BrushColor { set; }

        Color GradientBrushColor { set; }
        PointF GradientBrushPoint1 { set; }
        PointF GradientBrushPoint2 { set; }

        IDictionary<string, object> Globals { get; }

        IPen CreatePen();
        IBrush CreateBrush();

        PointF Project(PointF point);
        RectangleF Project(RectangleF rect);
        float Project(float number);

        void ResetTransform();
    }
}
