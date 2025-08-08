using SketchPen.Plot.Abstraction;
using SkiaSharp;
using System;

namespace SketchPen.Plot.Skia;

internal class PlotFont : IFont
{
    private readonly IPlotContext _context;
    public PlotFont(IPlotContext context, SKFont skFont)
    {
        _context = context;
        this.EngineElement = skFont;
    }
    public object EngineElement { get; }
    public void Dispose()
    {
        ((IDisposable)this.EngineElement).Dispose();
    }
}