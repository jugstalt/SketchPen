using SketchPen.Plot.Abstraction;
using System.Drawing;

namespace SketchPen.Plot.Drawing
{
    internal class PlotBrush : IBrush
    {
        private readonly PlotContext _plotContext;
        private readonly GraphicsContext _graphicsContext;
        private readonly bool _isPseudeoTransparent;

        public PlotBrush(PlotContext context, Brush brush, bool isPseudoTransparent)
        {
            _plotContext = context;
            _graphicsContext = (GraphicsContext)context.GraphicsContext;

            _isPseudeoTransparent = isPseudoTransparent;

            if (_isPseudeoTransparent)
            {
                _graphicsContext.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            }

            this.Brush = brush;
        }

        public Brush Brush { get; }

        public void Dispose()
        {
            this.Brush.Dispose();

            if (_isPseudeoTransparent)
            {
                _plotContext.Bitmap.MakeTransparent(PlotContext.TransparentColor);
                _graphicsContext.Graphics.SmoothingMode = PlotContext.DefaultSmothingMode;
            }
        }
    }
}