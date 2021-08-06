using SketchPen.Plot.Abstraction;
using System;
using System.Drawing;

namespace SketchPen.Plot
{
    internal class PlotBrush : IBrush
    {
        private readonly PlotContext _context;
        private readonly bool _isPseudeoTransparent;

        public PlotBrush(PlotContext context, Brush brush, bool isPseudoTransparent)
        {
            _context = context;
            _isPseudeoTransparent = isPseudoTransparent;

            if(_isPseudeoTransparent)
            {
                _context.GraphicsContext.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            }

            this.Brush = brush;
        }

        public Brush Brush { get; }

        public void Dispose()
        {
            this.Brush.Dispose();

            if(_isPseudeoTransparent)
            {
                _context.Bitmap.MakeTransparent(Plotter.TransparentColor);
                _context.GraphicsContext.SmoothingMode = Plotter.DefaultSmothingMode;
            }
        }
    }
}