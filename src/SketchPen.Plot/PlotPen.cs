using SketchPen.Plot.Abstraction;
using System;
using System.Drawing;

namespace SketchPen.Plot
{
    internal class PlotPen : IPen
    {
        private readonly PlotContext _context;

        public PlotPen(PlotContext context, Pen pen, bool isPseudoTransparent)
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