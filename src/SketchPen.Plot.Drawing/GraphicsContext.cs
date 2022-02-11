using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Drawing.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SketchPen.Plot.Drawing
{
    internal class GraphicsContext : IGraphicsContext
    {
        private readonly Graphics _graphics;

        public GraphicsContext(Bitmap bitmap)
        {
            _graphics = Graphics.FromImage(bitmap);

            _graphics.SmoothingMode = PlotContext.DefaultSmothingMode;
            _graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            //_graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            _graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            _graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        }

        internal Graphics Graphics => _graphics;

        #region IGraphicsContext

        public void DrawLine(IPen pen, CanvasPoint p1, CanvasPoint p2)
        {
            _graphics.DrawLine(((PlotPen)pen).Pen,
                new PointF(p1.X, p1.Y),
                new PointF(p2.X, p2.Y));
        }

        #endregion

        public void Dispose()
        {
            _graphics.Dispose();
        }

        public void TranslateTransform(float dx, float dy)
        {
            _graphics.TranslateTransform(dx, dy);
        }

        public void RotateTransform(float angle)
        {
            _graphics.RotateTransform(angle);
        }

        public void ScaleTransform(float sx, float sy)
        {
            _graphics.ScaleTransform(sx, sy);
        }

        public void DrawEllipse(IPen pen, CanvasRectangle rect)
        {
            _graphics.DrawEllipse(((PlotPen)pen).Pen, rect.ToRectangleF());
        }

        public void FillEllipse(IBrush brush, CanvasRectangle rect)
        {
            _graphics.FillEllipse(((PlotBrush)brush).Brush, rect.ToRectangleF());
        }

        public void DrawArc(IPen pen, CanvasRectangle rect, float startAngle, float sweepAngle)
        {
            _graphics.DrawArc(((PlotPen)pen).Pen, rect.ToRectangleF(), startAngle, sweepAngle);
        }

        public void FillPie(IBrush brush, CanvasRectangle rect, float startAngle, float sweepAngle)
        {
            _graphics.FillPie(((PlotBrush)brush).Brush, rect.ToRectangle(), startAngle, startAngle);
        }

        public void DrawPath(IPen pen, IPlotPath path)
        {
            _graphics.DrawPath(((PlotPen)pen).Pen, ((PlotPath)path).Path);
        }

        public void FillPath(IBrush brush, IPlotPath path)
        {
            _graphics.FillPath(((PlotBrush)brush).Brush, ((PlotPath)path).Path);
        }
    }
}
