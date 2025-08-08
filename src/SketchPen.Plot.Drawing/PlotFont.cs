using SketchPen.Plot.Abstraction;
using System.Drawing;

namespace SketchPen.Plot.Drawing;

internal class PlotFont : IFont
{
    private readonly IPlotContext _context;
    public PlotFont(IPlotContext context, Font font)
    {
        _context = context;
        this.EngineElement = font;
    }
    public object EngineElement { get; }
    public void Dispose()
    {
        if (EngineElement is Font)
        {
            ((Font)this.EngineElement).Dispose();
        }
    }
}