using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SketchPen.Plot
{
    internal class PlotContext : IPlotContext
    {
        private readonly Bitmap _bitmap;
        private readonly float _projectingFactor = 1f;

        public PlotContext(Bitmap bitmap)
        {
            _bitmap = bitmap;
            _projectingFactor = _bitmap.Width / 100f;

            this.GraphicsContext = Graphics.FromImage(_bitmap);
            this.ResetTransform();

            this.GradientBrushColor = Plotter.TransparentColor;
            this.GradientBrushPoint2 = new PointF(_bitmap.Width / 2f, _bitmap.Height / 2f);
            this.GradientBrushPoint1 = new PointF(-_bitmap.Width / 2f, -_bitmap.Height / 2f);

            MinPenWidth = 1f;
            MaxPenWidth = float.MaxValue;
            PenCap = PenCap.Round;

            this.Globals = new Dictionary<string, object>();
        }

        internal Bitmap Bitmap => _bitmap;

        #region IPlotContext

        public Graphics GraphicsContext { get; }

        public Color PenColor { private get; set; }
        public float PenWidth { private get; set; }
        public PenCap PenCap { private get; set; }
        public float MaxPenWidth { private get; set; }
        public float MinPenWidth { private get; set; }


        public Color BrushColor { private get; set; }

        public Color GradientBrushColor { private get; set; }
        public PointF GradientBrushPoint1 { private get; set; }
        public PointF GradientBrushPoint2 { private get; set; }

        public IDictionary<string, object> Globals { get; }

        public IPen CreatePen(IEnumerable<object> parameters = null)
        {
            float penWidth = parameters.Slice(1)?.ToTypedParameters<float>().FirstOrDefault() ?? this.PenWidth;

            if (penWidth >= MinPenWidth && penWidth <= MaxPenWidth)
            {
                penWidth = Project(this.PenWidth);
                if (penWidth < MinPenWidth)
                {
                    penWidth = MinPenWidth;
                }
                else if (penWidth > MaxPenWidth)
                {
                    penWidth = MaxPenWidth;
                }
            }

            var penColor = parameters.Slice(0)?.ToColor() ?? this.PenColor;
            var pen = new Pen(penColor, penWidth);

            switch (this.PenCap)
            {
                case PenCap.Round:
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.LineJoin = LineJoin.Round;
                    break;
                case PenCap.Square:
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Square;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Square;
                    pen.LineJoin = LineJoin.Bevel;
                    break;
                case PenCap.Flat:
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Flat;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Flat;
                    pen.LineJoin = LineJoin.Bevel;
                    break;
            }

            return new PlotPen(this, pen, penColor.Equals(Plotter.TransparentColor));
        }

        public IBrush CreateBrush(IEnumerable<object> parameters = null)
        {
            var brushColor = this.BrushColor;
            var gradientBrushColor = this.GradientBrushColor;

            if (parameters.CountElements() == 1)
            {
                brushColor = parameters.Slice(0).ToColor();
                gradientBrushColor = Plotter.TransparentColor;
            }
            else if (parameters.CountElements() == 2)
            {
                brushColor = parameters.Slice(0).ToColor();
                gradientBrushColor = parameters.Slice(1).ToColor();
            }

            Brush brush = null;
            if (!gradientBrushColor.Equals(Plotter.TransparentColor) &&
               this.GradientBrushPoint1 != null &&
               this.GradientBrushPoint2 != null)
            {
                brush = new LinearGradientBrush(
                    this.GradientBrushPoint1,
                    this.GradientBrushPoint2,
                    brushColor,
                    gradientBrushColor);
            }
            else
            {
                brush = new SolidBrush(brushColor);
            }

            return new PlotBrush(this, brush, brushColor.Equals(Plotter.TransparentColor));
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