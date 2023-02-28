using SketchPen.Plot.Abstraction;
using SkiaSharp;
using System;

namespace SketchPen.Plot.Skia;

internal class PlotPen : IPen
{
    private readonly IPlotContext _context;

    public PlotPen(IPlotContext context, SKPaint skPaint)
    {
        _context = context;
        this.EngineElement = skPaint;
    }

    public object EngineElement { get; }

    public void Dispose()
    {
        ((IDisposable)this.EngineElement).Dispose();
    }
}