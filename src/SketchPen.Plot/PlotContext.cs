using SketchPen.Plot.Abstraction;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SketchPen.Plot
{
    internal class PlotContext : IPlotContext
    {
        private readonly Bitmap _bitmap;
        private readonly float _projectingFactor = 1f;

        public PlotContext(Bitmap bitmap)
        {
            _bitmap = bitmap;
            _projectingFactor = (float)_bitmap.Width / 100f;

            this.GraphicsContext = Graphics.FromImage(_bitmap);
            this.ResetTransform();

            this.GradientBrushColor = Plotter.TransparentColor;
            this.GradientBrushPoint2 = new PointF(_bitmap.Width / 2f, _bitmap.Height / 2f);
            this.GradientBrushPoint1 = new PointF(-_bitmap.Width / 2f, -_bitmap.Height / 2f);

            this.Globals = new Dictionary<string, object>();
        }

        internal Bitmap Bitmap => _bitmap;

        #region

        public Graphics GraphicsContext { get; }

        public Color PenColor { private get; set; }
        public float PenWidth { private get; set; }
        public Color BrushColor { private get; set; }

        public Color GradientBrushColor { private get; set; }
        public PointF GradientBrushPoint1 { private get; set; }
        public PointF GradientBrushPoint2 { private get;  set; }

        public IDictionary<string, object> Globals { get; }

        public IPen CreatePen()
        {
            var pen = new Pen(this.PenColor, Project(this.PenWidth));
            pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            
            return new PlotPen(this, pen, this.PenColor.Equals(Plotter.TransparentColor));
        }
        public IBrush CreateBrush()
        {
            Brush brush = null;
            if (!this.GradientBrushColor.Equals(Plotter.TransparentColor) &&
               this.GradientBrushPoint1 != null &&
               this.GradientBrushPoint2 != null)
            {
                brush = new LinearGradientBrush(
                    this.GradientBrushPoint1,
                    this.GradientBrushPoint2,
                    this.BrushColor,
                    this.GradientBrushColor);
            }
            else
            {
                brush = new SolidBrush(this.BrushColor);
            }

            return new PlotBrush(this, brush, this.BrushColor.Equals(Plotter.TransparentColor));
        }

        public PointF Project(PointF point)
        {
            return new PointF(point.X * _projectingFactor, point.Y * _projectingFactor);
        }

        public RectangleF Project(RectangleF rect)
        {
            return new RectangleF(
                rect.Left * _projectingFactor,
                rect.Top * _projectingFactor,
                rect.Width * _projectingFactor,
                rect.Height * _projectingFactor);
        }

        public float Project(float number)
        {
            return number * _projectingFactor;
        }

        public void ResetTransform()
        {
            this.GraphicsContext.ResetTransform();
            this.GraphicsContext.TranslateTransform(Project(50f), Project(50f));
        }

        #endregion IPlotContext

        #region IDispable

        public void Dispose()
        {
            this.GraphicsContext.Dispose();
        }

        #endregion
    }
}