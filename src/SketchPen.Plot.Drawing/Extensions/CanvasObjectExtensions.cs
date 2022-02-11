using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SketchPen.Plot.Drawing.Extensions
{
    static class CanvasObjectExtensions
    {
        static public Point ToPoint(this CanvasPoint p)
            => new Point((int)p.X, (int)p.Y);

        static public PointF ToPointF(this CanvasPoint p)
            => new PointF(p.X, p.Y);

        static public Rectangle ToRectangle(this CanvasRectangle rectangleF)
            => new Rectangle((int)rectangleF.X, (int)rectangleF.Y, (int)rectangleF.Width, (int)rectangleF.Height);

        static public RectangleF ToRectangleF(this CanvasRectangle rectangleF)
            => new RectangleF(rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);

        static public Color ToColor(this PlotColor plotColor)
            => Color.FromArgb(plotColor.ToArgb());
    }
}
