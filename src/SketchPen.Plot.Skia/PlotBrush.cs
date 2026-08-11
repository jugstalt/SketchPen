using SketchPen.Plot.Abstraction;
using SkiaSharp;
using System;

namespace SketchPen.Plot.Skia;

internal class PlotBrush : IBrush
{
    private readonly IPlotContext _plotContext;
    private readonly Canvas _graphicsContext;
    private readonly bool _isPseudeoTransparent;

    public PlotBrush(IPlotContext context, SKPaint skPaint, bool isPseudoTransparent)
    {
        _plotContext = context;
        _graphicsContext = (Canvas)context.Canvas;

        _isPseudeoTransparent = isPseudoTransparent;

        //if (_isPseudeoTransparent)
        //{
        //    _graphicsContext.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        //}

        this.EngineElement = skPaint;
    }

    public object EngineElement { get; }

    public void Dispose()
    {
        ((IDisposable)this.EngineElement).Dispose();

        //if (_isPseudeoTransparent)
        //{
        //    _plotContext.Bitmap.MakeTransparent(PlotContext.TransparentColor);
        //    _graphicsContext.Graphics.SmoothingMode = PlotContext.DefaultSmothingMode;
        //}
    }
}