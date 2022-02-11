using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Drawing.Extensions;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SketchPen.Plot.Drawing
{
    class PlotPath : IPlotPath
    {
        private GraphicsPath _path;

        public PlotPath()
        {
            _path = new GraphicsPath();
        }

        internal GraphicsPath Path => _path;

        public void Start()
        {
            if (_path != null)
            {
                _path.StartFigure();
            }
        }

        public void Close()
        {
            if (_path != null)
            {
                _path.CloseFigure();
            }
        }

        public void AddArc(CanvasRectangle rect, float startAngle, float sweepAngle)
        {
            if (_path != null)
            {
                _path.AddArc(rect.ToRectangleF(), startAngle, sweepAngle);
            }
        }

        public void AddLines(IEnumerable<CanvasPoint> points)
        {
            if (_path != null)
            {
                _path.AddLines(points.Select(p => p.ToPointF()).ToArray());
            }
        }

        public void AddPoint(CanvasPoint point)
        {
            if (_path != null)
            {
                _path.AddLine(point.ToPointF(), point.ToPointF());
            }
        }

        public void Dispose()
        {
            if (_path != null)
            {
                _path.Dispose();
                _path = null;
            }
        }
    }
}
