using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Drawing.Extensions;
using SketchPen.Plot.Extensions;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SketchPen.Plot.Drawing
{
    public class PlotContext : IPlotContext
    {
        static internal Color TransparentColor = Color.Transparent; // Color.FromArgb(1, 0, 0);
        static internal System.Drawing.Drawing2D.SmoothingMode DefaultSmothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        private readonly Bitmap _bitmap;
        private readonly float _projectingFactor = 1f;

        public PlotContext(Bitmap bitmap)
        {
            _bitmap = bitmap;
            _projectingFactor = _bitmap.Width / 100f;

            this.GraphicsContext = new GraphicsContext(_bitmap);
            this.ResetTransform();

            this.GradientBrushColor = PlotColor.Transparent;
            this.GradientBrushPoint2 = new CanvasPoint(_bitmap.Width / 2f, _bitmap.Height / 2f);
            this.GradientBrushPoint1 = new CanvasPoint(-_bitmap.Width / 2f, -_bitmap.Height / 2f);

            MinPenWidth = 1f;
            MaxPenWidth = float.MaxValue;
            PenCap = PenCap.Round;

            this.Globals = new Dictionary<string, object>();


        }

        internal Bitmap Bitmap => _bitmap;

        #region IPlotContext

        public IGraphicsContext GraphicsContext { get; }

        public PlotColor PenColor { private get; set; }
        public float PenWidth { private get; set; }
        public PenCap PenCap { private get; set; }
        public float MaxPenWidth { private get; set; }
        public float MinPenWidth { private get; set; }


        public PlotColor BrushColor { private get; set; }

        public PlotColor GradientBrushColor { private get; set; }
        public CanvasPoint GradientBrushPoint1 { private get; set; }
        public CanvasPoint GradientBrushPoint2 { private get; set; }

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
            var pen = new Pen(penColor.ToColor(), penWidth);

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

            return new PlotPen(this, pen, penColor.Equals(PlotContext.TransparentColor));
        }

        public IBrush CreateBrush(IEnumerable<object> parameters = null)
        {
            var brushColor = this.BrushColor;
            var gradientBrushColor = this.GradientBrushColor;

            if (parameters.CountElements() == 1)
            {
                brushColor = parameters.Slice(0).ToColor();
                gradientBrushColor = PlotColor.Transparent;
            }
            else if (parameters.CountElements() == 2)
            {
                brushColor = parameters.Slice(0).ToColor();
                gradientBrushColor = parameters.Slice(1).ToColor();
            }

            Brush brush = null;
            if (!gradientBrushColor.Equals(PlotContext.TransparentColor) &&
               this.GradientBrushPoint1 != null &&
               this.GradientBrushPoint2 != null)
            {
                brush = new LinearGradientBrush(
                    this.GradientBrushPoint1.ToPoint(),
                    this.GradientBrushPoint2.ToPoint(),
                    brushColor.ToColor(),
                    gradientBrushColor.ToColor());
            }
            else
            {
                brush = new SolidBrush(brushColor.ToColor());
            }

            return new PlotBrush(this, brush, brushColor.Equals(PlotContext.TransparentColor));
        }

        public CanvasPoint Project(CanvasPoint point)
        {
            return new CanvasPoint(point.X * _projectingFactor, point.Y * _projectingFactor);
        }

        public CanvasRectangle Project(CanvasRectangle rect)
        {
            return new CanvasRectangle(
                rect.X * _projectingFactor,
                rect.Y * _projectingFactor,
                rect.Width * _projectingFactor,
                rect.Height * _projectingFactor);
        }

        public float Project(float number)
        {
            return number * _projectingFactor;
        }

        public void ResetTransform()
        {
            ((GraphicsContext)this.GraphicsContext).Graphics.ResetTransform();
            ((GraphicsContext)this.GraphicsContext).Graphics.TranslateTransform(Project(50f), Project(50f));
        }

        #endregion IPlotContext

        #region IDispable

        public void Dispose()
        {
            this.GraphicsContext.Dispose();
        }

        public IPlotPath CreatePlotPath() => new PlotPath();

        #endregion
    }
}
