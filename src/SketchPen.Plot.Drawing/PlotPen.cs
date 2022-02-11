using SketchPen.Plot.Abstraction;
using System.Drawing;

namespace SketchPen.Plot.Drawing
{
    internal class PlotPen : IPen
    {
        private readonly IPlotContext _context;

        public PlotPen(IPlotContext context, Pen pen, bool isPseudoTransparent)
        {
            _context = context;
            this.Pen = pen;
        }

        public Pen Pen { get; }

        public void Dispose()
        {
            this.Pen.Dispose();
        }
    }
}