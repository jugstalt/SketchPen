using SketchPen.Plot.Abstraction;
using System.Drawing;

namespace SketchPen.Plot.Drawing;

internal class PlotPen : IPen
{
    private readonly IPlotContext _context;

    public PlotPen(IPlotContext context, Pen pen, bool isPseudoTransparent)
    {
        _context = context;
        this.EngineElement = pen;
    }

    public object EngineElement { get; }

    public void Dispose()
    {
        if (EngineElement is Pen)
        {
            ((Pen)this.EngineElement).Dispose();
        }
    }
}